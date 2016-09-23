Public Class BlurForm
    Dim OriginalBitmap As Bitmap

    Private Sub RadiusBar_Scroll(sender As Object, e As EventArgs) Handles RadiusBar.Scroll
        Dim FirstTime As Integer = My.Computer.Clock.TickCount
        Dim MyGaussianBlur As FastGaussianBlur = New FastGaussianBlur(OriginalBitmap)
        BackgroundImage = MyGaussianBlur.GaussianBlur(RadiusBar.Value)
        Dim SecondTime As Integer = My.Computer.Clock.TickCount
        Me.Text = "快速高斯模糊 - 模糊半径：" & RadiusBar.Value & "，用时：" & (SecondTime - FirstTime) & " 毫秒"
        MyGaussianBlur = Nothing
        GC.Collect()
    End Sub

    Private Sub BlurForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        OriginalBitmap = Me.BackgroundImage
        RadiusBar_Scroll(sender, e)
    End Sub

    Private Sub BlurForm_Click(sender As Object, e As EventArgs) Handles Me.Click
        Dim FileName As String = "D:\GaussianBlur-" & RadiusBar.Value & "-" & My.Computer.Clock.TickCount & ".png"
        Me.BackgroundImage.Save(FileName)
        Me.Text = "文件已经储存在 " & FileName
    End Sub
End Class
