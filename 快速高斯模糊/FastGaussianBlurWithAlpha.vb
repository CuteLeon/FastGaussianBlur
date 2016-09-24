Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class FastGaussianBlurWithAlpha
    Private ReadOnly OriginalRed As Integer()
    Private ReadOnly OriginalGreen As Integer()
    Private ReadOnly OriginalBlue As Integer()
    Private ReadOnly OriginalAlpha As Integer()
    Private ReadOnly BitmapWidth As Integer
    Private ReadOnly BitmapHeight As Integer
    Private ReadOnly TaskParalleOptions As ParallelOptions = New ParallelOptions With {.MaxDegreeOfParallelism = 16}

    Public Sub New(OriginalBitmap As Bitmap)
        Dim BitmapRectangle As Rectangle = New Rectangle(0, 0, OriginalBitmap.Width, OriginalBitmap.Height)
        Dim Source(BitmapRectangle.Width * BitmapRectangle.Height - 1) As Integer
        Dim BitmapBits As BitmapData = OriginalBitmap.LockBits(BitmapRectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Marshal.Copy(BitmapBits.Scan0, Source, 0, Source.Length)
        OriginalBitmap.UnlockBits(BitmapBits)
        BitmapWidth = OriginalBitmap.Width : BitmapHeight = OriginalBitmap.Height
        OriginalRed = New Integer(BitmapWidth * BitmapHeight - 1) {}
        OriginalGreen = New Integer(BitmapWidth * BitmapHeight - 1) {}
        OriginalBlue = New Integer(BitmapWidth * BitmapHeight - 1) {}
        OriginalAlpha = New Integer(BitmapWidth * BitmapHeight - 1) {}
        Parallel.[For](0, Source.Length, TaskParalleOptions,
            Sub(Index As Integer)
                OriginalAlpha(Index) = (Source(Index) And 4261478400) >> 24
                OriginalRed(Index) = (Source(Index) And 16711680) >> 16
                OriginalGreen(Index) = (Source(Index) And 65280) >> 8
                OriginalBlue(Index) = (Source(Index) And 255)
            End Sub)
    End Sub

    Public Function GaussianBlur(Radius As Integer) As Bitmap
        '最大模糊直径不允许等于或大于图像的宽度或高度中的最小值
        If Radius >= BitmapWidth / 2 Or Radius >= BitmapHeight / 2 Then Radius = Math.Min(BitmapWidth, BitmapHeight) / 2 - 1

        Dim NewRed(BitmapWidth * BitmapHeight - 1) As Integer
        Dim NewGreen(BitmapWidth * BitmapHeight - 1) As Integer
        Dim NewBlue(BitmapWidth * BitmapHeight - 1) As Integer
        Dim NewAlpha(BitmapWidth * BitmapHeight - 1) As Integer
        Dim DataArray(BitmapWidth * BitmapHeight - 1) As Integer '使用一个4字节的Integer整型同时储存ARGB四个Byte数据，一次写入内存

        Parallel.Invoke(New Action() {
            Sub()
                ShellBlur(OriginalRed, NewRed, Radius)
            End Sub,
            Sub()
                ShellBlur(OriginalGreen, NewGreen, Radius)
            End Sub,
            Sub()
                ShellBlur(OriginalBlue, NewBlue, Radius)
            End Sub,
            Sub()
                ShellBlur(OriginalAlpha, NewAlpha, Radius)
            End Sub
                        })

        Parallel.[For](0, DataArray.Length, TaskParalleOptions,
            Sub(Index As Integer)
                If NewRed(Index) > 255 Then NewRed(Index) = 255
                If NewGreen(Index) > 255 Then NewGreen(Index) = 255
                If NewBlue(Index) > 255 Then NewBlue(Index) = 255
                If NewAlpha(Index) > 255 Then NewAlpha(Index) = 255
                If NewRed(Index) < 0 Then NewRed(Index) = 0
                If NewGreen(Index) < 0 Then NewGreen(Index) = 0
                If NewBlue(Index) < 0 Then NewBlue(Index) = 0
                If NewAlpha(Index) < 0 Then NewAlpha(Index) = 0
                DataArray(Index) = (NewAlpha(Index) << 24 Or NewRed(Index) << 16 Or NewGreen(Index) << 8 Or NewBlue(Index))
            End Sub)

        Dim NewBitmap As Bitmap = New Bitmap(BitmapWidth, BitmapHeight)
        Dim BitmapRectangle As Rectangle = New Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height)
        Dim BiamapBits As BitmapData = NewBitmap.LockBits(BitmapRectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Marshal.Copy(DataArray, 0, BiamapBits.Scan0, DataArray.Length)
        NewBitmap.UnlockBits(BiamapBits)
        GC.SuppressFinalize(Me)
        Return NewBitmap
    End Function

    Private Sub ShellBlur(Source As Integer(), DataArray As Integer(), Radius As Integer)
        Dim Boxs As Integer() = BoxesForGauss(Radius, 3)
        BoxBlur(Source, DataArray, BitmapWidth, BitmapHeight, (Boxs(0) - 1) / 2)
        BoxBlur(DataArray, Source, BitmapWidth, BitmapHeight, (Boxs(1) - 1) / 2)
        BoxBlur(Source, DataArray, BitmapWidth, BitmapHeight, (Boxs(2) - 1) / 2)
    End Sub

    Private Function BoxesForGauss(Sigma As Integer, N As Integer) As Integer()
        Dim WIdeal As Double = Math.Sqrt(CDec((12 * Sigma * Sigma / N + 1)))
        Dim WL As Integer = CInt(Math.Floor(WIdeal))
        If WL Mod 2 = 0 Then WL -= 1
        Dim WU As Integer = WL + 2
        Dim MIdeal As Double = CDec((12 * Sigma * Sigma - N * WL * WL - 4 * N * WL - 3 * N)) / CDec((-4 * WL - 4))
        Dim Index As Double = Math.Round(MIdeal)
        Dim Sizes As List(Of Integer) = New List(Of Integer)()
        For IndexN As Integer = 0 To N - 1
            Sizes.Add(If((CDec(IndexN) < Index), WL, WU))
        Next
        Return Sizes.ToArray()
    End Function

    Private Sub BoxBlur(Source As Integer(), DataArray As Integer(), Width As Integer, Height As Integer, Radius As Integer)
        For Index As Integer = 0 To Source.Length - 1
            DataArray(Index) = Source(Index)
        Next
        BoxBlurH(DataArray, Source, Width, Height, Radius)
        BoxBlurT(Source, DataArray, Width, Height, Radius)
    End Sub

    Private Sub BoxBlurH(Source As Integer(), DataArray As Integer(), Width As Integer, Height As Integer, Radius As Integer)
        Dim IARadius As Double = 1.0 / CDec((Radius + Radius + 1))
        Parallel.[For](0, Height, TaskParalleOptions,
            Sub(Index As Integer)
                Dim CountInLine As Integer = Index * Width
                Dim LengthLine As Integer = CountInLine
                Dim LineLength As Integer = CountInLine + Radius
                Dim FirstValue As Integer = Source(CountInLine)
                Dim LastValue As Integer = Source(CountInLine + Width - 1)
                Dim TempValue As Integer = (Radius + 1) * FirstValue
                For Index = 0 To Radius - 1
                    TempValue += Source(CountInLine + Index)
                Next
                For Index = 0 To Radius
                    TempValue = TempValue + (Source(LineLength) - FirstValue)
                    DataArray(CountInLine) = CInt(Math.Round(CDec(TempValue) * IARadius))
                    LineLength += 1 : CountInLine += 1
                Next
                For Index = Radius + 1 To Width - Radius - 1
                    TempValue = TempValue + (Source(LineLength) - DataArray(LengthLine))
                    DataArray(CountInLine) = CInt(Math.Round(CDec(TempValue) * IARadius))
                    CountInLine += 1 : LengthLine += 1 : LineLength += 1
                Next
                For Index = Width - Radius To Width - 1
                    TempValue = TempValue + (LastValue - Source(LengthLine))
                    DataArray(CountInLine) = CInt(Math.Round(CDec(TempValue) * IARadius))
                    LengthLine += 1 : CountInLine += 1
                Next
            End Sub)
    End Sub

    Private Sub BoxBlurT(Source As Integer(), DataArray As Integer(), Width As Integer, Height As Integer, Radius As Integer)
        Dim IARadius As Double = 1.0 / CDec((Radius + Radius + 1))
        Parallel.[For](0, Width, TaskParalleOptions,
            Sub(Index As Integer)
                Dim CountInLine As Integer = Index
                Dim LengthLine As Integer = CountInLine
                Dim LineLength As Integer = CountInLine + Radius * Width
                Dim FirstValue As Integer = Source(CountInLine)
                Dim LastValue As Integer = Source(CountInLine + Width * (Height - 1))
                Dim TempValue As Integer = (Radius + 1) * FirstValue
                For Index = 0 To Radius - 1
                    TempValue += Source(CountInLine + Index * Width)
                Next
                For Index = 0 To Radius
                    TempValue += Source(LineLength) - FirstValue
                    DataArray(CountInLine) = CInt(Math.Round(CDec(TempValue) * IARadius))
                    LineLength += Width
                    CountInLine += Width
                Next
                For Index = Radius + 1 To Height - Radius - 1
                    TempValue += Source(LineLength) - Source(LengthLine)
                    DataArray(CountInLine) = CInt(Math.Round(CDec(TempValue) * IARadius))
                    LengthLine += Width
                    LineLength += Width
                    CountInLine += Width
                Next
                For Index = Height - Radius To Height - 1
                    TempValue += LastValue - Source(LengthLine)
                    DataArray(CountInLine) = CInt(Math.Round(CDec(TempValue) * IARadius))
                    LengthLine += Width
                    CountInLine += Width
                Next
            End Sub)
    End Sub

End Class
