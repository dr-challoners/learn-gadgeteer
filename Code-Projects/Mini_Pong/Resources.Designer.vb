'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.1
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------


    Partial Friend Class Resources
        Private Shared manager As System.Resources.ResourceManager

        Friend Shared ReadOnly Property ResourceManager() As System.Resources.ResourceManager
            Get
                If (Resources.manager Is Nothing) Then
                    Resources.manager = New System.Resources.ResourceManager("Mini_Pong.Resources", GetType(Resources).Assembly)
                End If
                Return Resources.manager
            End Get
        End Property

        Friend Shared Function GetFont(id As Resources.FontResources) As Microsoft.SPOT.Font
            Return DirectCast(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id), Microsoft.SPOT.Font)
        End Function

        <System.SerializableAttribute()> _
        Friend Enum FontResources As Short
            small = 13070
            NinaB = 18060
        End Enum
    End Class