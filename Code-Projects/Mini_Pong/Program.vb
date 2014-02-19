Imports GT = Gadgeteer
Imports GTM = Gadgeteer.Modules
Imports Gadgeteer.Modules.Seeed
Imports Gadgeteer.Modules.GHIElectronics
Imports GHI.OSHW.Hardware

Partial Public Class Program

    ' -- Colour Definitions --
    Public bat_Colour As Microsoft.SPOT.Presentation.Media.Color = GT.Color.Red
    Public ball_Colour As Microsoft.SPOT.Presentation.Media.Color = GT.Color.Blue
    Public back_Colour As Microsoft.SPOT.Presentation.Media.Color = GT.Color.Black

    ' -- Object Sizes --
    Public bat_Width As Integer = 20
    Public bat_Height As Integer = 4
    Public ball_Width As Integer = 2
    Public ball_Height As Integer = 2

    ' -- Screen Sizes --
    Public x_Min As Integer = 0
    Public x_Max As Integer = 127
    Public y_Min As Integer = 0
    Public y_Max As Integer = 127

    ' -- Game Objects --
    Public timer As GT.Timer
    Public randomiser As New System.Random()
    Public current_Bat_X As Integer
    Public current_Bat_Y As Integer
    Public current_Ball_X As Integer
    Public current_Ball_Y As Integer

    ' -- Speed Scales --
    Public bat_Speed As Integer = 20
    Public ball_Speed As Integer
    Public ball_Speed_X As Integer
    Public ball_Speed_Y As Integer = ball_Speed

    ' -- Progress --
    Public current_Hits As Integer = 0
    Public games_Left As Integer = 6

    Public Sub ProgramStarted()

        Debug.Print("Gravy...")

        Initialise_Game()

    End Sub

    Public Sub Initialise_Game()

        ' -- Clear Timer if the game is already in progress --
        If Not timer Is Nothing Then
            If timer.IsRunning Then timer.Stop()
            timer = Nothing
            Draw_Rectangle(current_Bat_X, current_Bat_Y, current_Bat_X + bat_Width, current_Bat_Y + bat_Height, back_Colour)
            Draw_Rectangle(current_Ball_X, current_Ball_Y, current_Ball_X + ball_Width, current_Ball_Y + ball_Height, back_Colour)
            current_Hits = 0
            games_Left -= 1
        End If

        ' -- Show Lives --
        For i As Integer = 1 To 6
            If games_Left >= i Then
                led7r.TurnLightOn(i)
            Else
                led7r.TurnLightOff(i)
            End If
        Next

        ' -- Bat Starting Position (Top Left of Bat) --
        current_Bat_X = CInt((x_Max / 2) - (bat_Width / 2))
        current_Bat_Y = CInt(y_Max - bat_Height)

        ' -- Ball Starting Position -- 
        current_Ball_X = randomiser.Next(x_Max - ball_Width)
        current_Ball_Y = 0

        ' -- Set Up Ball Speed --
        ball_Speed = 2
        If randomiser.Next(99) >= 50 Then
            ball_Speed_X = ball_Speed
        Else
            ball_Speed_X = 0 - ball_Speed
        End If
        ball_Speed_Y = ball_Speed

        ' -- Draw Bat & Ball --
        Draw_Bat(current_Bat_X, current_Bat_Y)
        Draw_Ball(current_Ball_X, current_Ball_Y)

        ' -- Create the Timer --
        If games_Left > 0 Then
            timer = New GT.Timer(50)
            AddHandler timer.Tick, AddressOf handle_Timer_Tick
        End If

    End Sub

    Public Sub Draw_Rectangle( _
        ByVal start_X As Integer, _
        ByVal start_Y As Integer, _
        ByVal end_X As Integer, _
        ByVal end_Y As Integer, _
        ByVal col As Microsoft.SPOT.Presentation.Media.Color _
    )

        Dim rect_Bitmap As New Bitmap(end_X - start_X, end_Y - start_Y)

        Dim rect_Buffer As Byte() = New Byte(rect_Bitmap.Width * rect_Bitmap.Height * 2 - 1) {}

        rect_Bitmap.DrawRectangle(col, 0, 0, 0, rect_Bitmap.Width, rect_Bitmap.Height, 0, 0, col, 0, 0, col, 0, 0, 100)

        Util.BitmapConvertBPP(rect_Bitmap.GetBitmap(), rect_Buffer, Util.BPP_Type.BPP16_BGR_BE)

        oledDisplay.FlushRawBitmap(CByte(start_X), CByte(start_Y), CByte(rect_Bitmap.Width), CByte(rect_Bitmap.Height), rect_Buffer)

    End Sub

    Public Sub Draw_Ball( _
        ByVal x As Integer, _
        ByVal y As Integer _
    )

        If current_Ball_X <> x OrElse current_Ball_Y <> y Then _
                Draw_Rectangle(current_Ball_X, current_Ball_Y, current_Ball_X + ball_Width, current_Ball_Y + ball_Height, back_Colour) ' Blank old position

        ' Left/Right Side X-Side Detection
        If x <= 0 Then
            x = 0
            ball_Speed_X = ball_Speed
        ElseIf (x + ball_Width) >= x_Max Then
            x = x_Max - ball_Width
            ball_Speed_X = 0 - ball_Speed
        End If

        ' Top/Bottom Y-Side Detection
        If y <= 0 Then
            y = 0
            ball_Speed_Y = ball_Speed
        ElseIf (y + ball_Height) >= current_Bat_Y AndAlso (x >= current_Bat_X And x <= current_Bat_X + bat_Width) Then
            y = current_Bat_Y - ball_Height
            ball_Speed_Y = 0 - ball_Speed
            current_Hits += 1
            If current_Hits > 10 AndAlso games_Left < 6 Then
                games_Left += 1
                current_Hits = 0
            End If
            ball_Speed += 1
        ElseIf (y + ball_Height) >= y_Max Then
            Initialise_Game()
            Return
        End If

        Draw_Rectangle(x, y, x + ball_Width, y + ball_Height, ball_Colour)

        current_Ball_X = x
        current_Ball_Y = y

    End Sub

    Public Sub Draw_Bat( _
        ByVal x As Integer, _
        ByVal y As Integer _
    )

        ' -- Check Requested Bat Position is within Playable Boundaries --
        If x >= 0 AndAlso (x + bat_Width) <= x_Max AndAlso _
            y >= y AndAlso (y + bat_Height <= y_Max) Then

            If current_Bat_X <> x OrElse current_Bat_Y <> y Then _
                Draw_Rectangle(current_Bat_X, current_Bat_Y, current_Bat_X + bat_Width, current_Bat_Y + bat_Height, back_Colour) ' Blank old position

            Draw_Rectangle(x, y, x + bat_Width, y + bat_Height, bat_Colour)

            current_Bat_X = x
            current_Bat_Y = y

        End If

    End Sub

    Public Sub handle_Timer_Tick( _
        ByVal timer As GT.Timer _
    )

        Dim x As Double = joystick.GetJoystickPosition().X
        Dim y As Double = joystick.GetJoystickPosition().Y

        Dim deflection As Integer = 0

        If x < 0.45 Then
            deflection = 0 - CInt((0.5 - x) * bat_Speed)
        ElseIf x > 0.55 Then
            deflection = CInt((x - 0.5) * bat_Speed)
        End If

        If deflection <> 0 Then Draw_Bat(current_Bat_X + deflection, current_Bat_Y)

        Draw_Ball(current_Ball_X + ball_Speed_X, current_Ball_Y + ball_Speed_Y)

    End Sub

    Public Sub handle_Joystick_Press( _
        ByVal sender As Gadgeteer.Modules.GHIElectronics.Joystick, _
        ByVal state As Gadgeteer.Modules.GHIElectronics.Joystick.JoystickState _
    ) Handles joystick.JoystickPressed

        If timer.IsRunning Then
            bat_Colour = GT.Color.Red
            timer.Stop()
        Else
            bat_Colour = GT.Color.Green
            timer.Start()
        End If

        Draw_Bat(current_Bat_X, current_Bat_Y)

    End Sub

End Class