Imports GT = Gadgeteer
Imports GTM = Gadgeteer.Modules
Imports Gadgeteer.Modules.GHIElectronics
Imports Gadgeteer.Modules.Seeed
Imports GHI.OSHW.Hardware

Partial Public Class Program

    ' This is the number of colour blocks on the X and Y axis (e.g. 4 * 4 will be 16 blocks overall
    Private m_BlockCount As Integer = 4

    ' This is the amount to change the colour by for each block (positive will get lighter, negative darker
    Private m_ColourIncrement As Integer = 4

    ' Whether we'd like to output all messages to the Debug Window
    Private m_Debug As System.Boolean = False

    Public Sub ProgramStarted()

        ' Say Hello to the World!
        Debug.Print("Program Started")

    End Sub

    ' Create a timer that will fire 5 times every second (per 200ms)
    Dim WithEvents timer As GT.Timer = New GT.Timer(200)

    ' Runs on Every 'Tick' of the Timer (this is where we will measure the joystick position and update the screen)
    Private Sub timer_Tick( _
        ByVal timer As Gadgeteer.Timer _
    ) Handles timer.Tick

        ' Gets the current joystick position (where both x and y are between 0 and 1)
        Dim x As Double = joystick.GetJoystickPosition().X
        Dim y As Double = joystick.GetJoystickPosition().Y
        If m_Debug Then Debug.Print("Joystick X:" & System.Math.Round(x) & ", Y:" & System.Math.Round(y))

        ' 4 Quandrants, 3 colours, doesn't really work - but we can try!
        ' Fill screen with appropriate colour, using the x/y position of the joystick to create a colour
        ' represented by 0-255 for each of the Red, Green & Blue Channels
        If x >= 0.5 Then
            If y >= 0.5 Then
                fill_Screen(CByte(Byte.MaxValue * y), CByte(Byte.MaxValue * x), 0)
            Else
                fill_Screen(0, CByte(Byte.MaxValue * x), CByte(Byte.MaxValue * y))
            End If
        Else
            If y >= 0.5 Then
                fill_Screen(CByte(Byte.MaxValue * y), 0, CByte(Byte.MaxValue * x))
            Else
                fill_Screen(CByte(Byte.MaxValue * y), CByte(Byte.MaxValue * x), CByte(Byte.MaxValue * y))
            End If
        End If

    End Sub

    ' Method to actually fill the screen (square by square)
    Private Sub fill_Screen( _
        ByVal red As Byte, _
        ByVal green As Byte, _
        ByVal blue As Byte _
    )

        ' Check whether the timer is currently running or not.
        Dim intitial_State As Boolean = timer.IsRunning

        If m_Debug Then Debug.Print("Filling Screen with Red:" & red & ", Green:" & green & ", Blue:" & blue)

        Try
            ' Work out how many pixels wide/high the blocks have to be to fit the correct number in!
            Dim block_Width As Byte = CByte(oledDisplay.Width / m_BlockCount)
            Dim block_Height As Byte = CByte(oledDisplay.Height / m_BlockCount)

            ' Loop through each column
            For i As Byte = 0 To CByte(m_BlockCount - 1)

                ' If the timer state has changed, stop updating the screen!
                If Not timer.IsRunning = intitial_State Then Exit For

                ' Loop through each row
                For j As Byte = 0 To CByte(m_BlockCount - 1)

                    ' If the timer state has changed, stop updating the screen!
                    If Not timer.IsRunning = intitial_State Then Exit For

                    ' Draw the rectangle, using the i and j variables to shift to a new block each time!
                    Draw_Rectangle(i * block_Width, j * block_Height, (i + 1) * block_Width, (j + 1) * block_Height, _
                        GT.Color.FromRGB(red, green, blue))

                    ' If the colour is black, don't change the colours....
                    If Not (red = 0 AndAlso green = 0 AndAlso blue = 0) Then

                        ' Change the colours each cycle, using Math.Min and Math.Max to keep the values between 0 and 255
                        ' CByte just changes the colour value to a Byte each time, rather than an Integer
                        red = CByte(System.Math.Max(0, System.Math.Min(red + m_ColourIncrement, 255)))
                        green = CByte(System.Math.Max(0, System.Math.Min(green + m_ColourIncrement, 255)))
                        blue = CByte(System.Math.Max(0, System.Math.Min(blue + m_ColourIncrement, 255)))

                    End If
                    
                Next

            Next

        Catch ex As Exception

            ' In case or error - print out what went wrong!
            Debug.Print("Errored:" & ex.ToString)
            Debug.Print(ex.StackTrace)

        End Try

    End Sub

    ' If the joystick is pressed/released, then start or stop the timer (depending on it's current state)
    Private Sub joystick_JoystickReleased( _
        ByVal sender As Gadgeteer.Modules.GHIElectronics.Joystick, _
        ByVal state As Gadgeteer.Modules.GHIElectronics.Joystick.JoystickState _
    ) Handles joystick.JoystickReleased

        If timer.IsRunning Then
            timer.Stop()
            fill_Screen(0, 0, 0) ' Paint the Screen Black (e.g. turn off)
        Else
            timer.Start()
        End If

    End Sub

    ' This method does the actual drawing of a rectangle between four points.
    Public Sub Draw_Rectangle( _
        ByVal start_X As Integer, _
        ByVal start_Y As Integer, _
        ByVal end_X As Integer, _
        ByVal end_Y As Integer, _
        ByVal col As Microsoft.SPOT.Presentation.Media.Color _
    )

        ' Create the Rectangular Bitmap, sizing it appropriately
        Dim rect_Bitmap As New Bitmap(end_X - start_X, end_Y - start_Y)

        ' Create an array of Bytes to hold pixel values
        Dim rect_Buffer As Byte() = New Byte(rect_Bitmap.Width * rect_Bitmap.Height * 2 - 1) {}

        ' Draws the actual rectangle within the bitmap (actually just fills the bitmap with colour)
        rect_Bitmap.DrawRectangle(col, 0, 0, 0, rect_Bitmap.Width, rect_Bitmap.Height, 0, 0, col, 0, 0, col, 0, 0, 100)

        ' Converts the bitmap to appropriate Byte Array to send to Screen
        Util.BitmapConvertBPP(rect_Bitmap.GetBitmap(), rect_Buffer, Util.BPP_Type.BPP16_BGR_BE)

        ' Sends the Byte Array to the Screen, putting it at the right point on the screen
        oledDisplay.FlushRawBitmap(CByte(start_X), CByte(start_Y), CByte(rect_Bitmap.Width), CByte(rect_Bitmap.Height), rect_Buffer)

        ' Clears up the objects as memory is scarce and we need everything we can get!
        ' The Garbage Collector will come along and free the memory shortly.....
        rect_Bitmap.Clear()
        rect_Bitmap.Dispose()
        rect_Bitmap = Nothing
        rect_Buffer = Nothing

    End Sub

End Class