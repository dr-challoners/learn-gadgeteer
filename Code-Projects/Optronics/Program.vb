Imports GT = Gadgeteer
Imports GTM = Gadgeteer.Modules
Imports Gadgeteer.Modules.GHIElectronics


Partial Public Class Program

    ' This is run when the mainboard is powered up or reset. 
    Public Sub ProgramStarted()
        '*******************************************************************************************
        ' Hardware modules added in the Program.gadgeteer designer view are used by typing 
        ' their name followed by a period, e.g.  button.  or  camera.
        '
        ' Many hardware modules generate useful events. To set up actions for those events, use the 
        ' left dropdown box at the top of this code editing window to choose a hardware module, then
        ' use the right dropdown box to choose the event - an event handler will be auto-generated.
        '*******************************************************************************************/

        ' Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
        Debug.Print("Program Started")

        ' Calibrate for LOW light Setting
        Thread.Sleep(100) ' Wait for a few moments
        low_Threshold = CInt(lightSensor.ReadLightSensorPercentage() + (lightSensor.ReadLightSensorPercentage() * 0.5))
        Debug.Print("Calibrated Low Threshold Reading As: " & low_Threshold)

        ' Calibrate for HIGH light Setting
        Switch_Led(True)
        Thread.Sleep(100)
        high_Threshold = CInt(lightSensor.ReadLightSensorPercentage() - (lightSensor.ReadLightSensorPercentage() * 0.1))
        Debug.Print("Calibrated High Threshold Reading As: " & high_Threshold)

        ' Reset to Normal State
        Switch_Led(False)
        Thread.Sleep(100)

        ' Start Receiving
        receiving_Timer.Start()

    End Sub

    ' If you want to do something periodically, declare a GT.Timer by uncommenting the below line
    '   and then use the dropdown boxes at the top of this window to generate a Tick event handler.
    Dim WithEvents receiving_Timer As GT.Timer = New GT.Timer(5)  ' every second (1000ms)

    Dim low_Threshold As Integer
    Dim high_Threshold As Integer
    Dim last_High As Boolean = False

    Dim _debug As Boolean = False

    Private Sub button_ButtonPressed(ByVal sender As Gadgeteer.Modules.GHIElectronics.Button, ByVal state As Gadgeteer.Modules.GHIElectronics.Button.ButtonState) Handles button.ButtonPressed
        Switch_Led(True)
    End Sub

    Private Sub button_ButtonReleased(ByVal sender As Gadgeteer.Modules.GHIElectronics.Button, ByVal state As Gadgeteer.Modules.GHIElectronics.Button.ButtonState) Handles button.ButtonReleased
        Switch_Led(False)
    End Sub

    Private Sub receiving_Timer_Tick(ByVal timer As Gadgeteer.Timer) Handles receiving_Timer.Tick
        Dim current_Reading As Double = lightSensor.ReadLightSensorPercentage()
        If _debug Then Debug.Print(current_Reading.ToString)
        If current_Reading > high_Threshold AndAlso Not last_High Then
            ' High State Change
            last_High = True
            RaiseEvent State_Changed(True)
        ElseIf current_Reading < low_Threshold AndAlso last_High Then
            ' Low State Change
            last_High = False
            RaiseEvent State_Changed(False)
        Else
            ' No State Change
        End If
    End Sub

    Private Event State_Changed(ByVal high As Boolean)

    Private Sub Handle_Received(ByVal high As Boolean) Handles Me.State_Changed
        If high Then
            Debug.Print("Gone High")
        Else
            Debug.Print("Gone Low")
        End If
    End Sub

    Private Sub Switch_Led( _
        ByVal _on As Boolean _
    )
        For i As Integer = 1 To 7
            If _on Then
                led7r.TurnLightOn(i)
            Else
                led7r.TurnLightOff(i)
            End If
        Next
    End Sub
End Class
