<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BlurForm
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(BlurForm))
        Me.RadiusBar = New System.Windows.Forms.TrackBar()
        CType(Me.RadiusBar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'RadiusBar
        '
        Me.RadiusBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.RadiusBar.LargeChange = 15
        Me.RadiusBar.Location = New System.Drawing.Point(0, 0)
        Me.RadiusBar.Maximum = 272
        Me.RadiusBar.Name = "RadiusBar"
        Me.RadiusBar.Size = New System.Drawing.Size(553, 45)
        Me.RadiusBar.SmallChange = 5
        Me.RadiusBar.TabIndex = 0
        Me.RadiusBar.TickFrequency = 5
        Me.RadiusBar.Value = 20
        '
        'BlurForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.ClientSize = New System.Drawing.Size(553, 554)
        Me.Controls.Add(Me.RadiusBar)
        Me.DoubleBuffered = True
        Me.Name = "BlurForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "快速高斯模糊"
        CType(Me.RadiusBar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents RadiusBar As TrackBar
End Class
