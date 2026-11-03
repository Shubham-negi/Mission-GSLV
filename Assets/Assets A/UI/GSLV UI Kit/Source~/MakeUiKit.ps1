# Draws the GSLV UI kit (the reusable components of the Mission GSLV UI sheets) as PNG sprites into the folder above.
# Unity ignores this Source~ folder. The Timeline builder sets the sprites' import settings (9-slice borders etc.).
# Run: powershell -ExecutionPolicy Bypass -File MakeUiKit.ps1
param([string]$Out = (Join-Path $PSScriptRoot '..'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$Out = [System.IO.Path]::GetFullPath($Out)
$script:made = @()

# ------------------------------------------------------------------ helpers
function Col([int]$r, [int]$g, [int]$b, [double]$a = 1) { [System.Drawing.Color]::FromArgb([int][math]::Round($a * 255), $r, $g, $b) }
function Pt([double]$x, [double]$y) { New-Object System.Drawing.PointF([float]$x, [float]$y) }
function Rect([double]$x, [double]$y, [double]$w, [double]$h) { New-Object System.Drawing.RectangleF([float]$x, [float]$y, [float]$w, [float]$h) }
$White = Col 255 255 255

function New-Bmp([int]$w, [int]$h) {
    $bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.InterpolationMode = 'HighQualityBicubic'; $g.PixelOffsetMode = 'HighQuality'
    $g.CompositingQuality = 'HighQuality'; $g.TextRenderingHint = 'AntiAliasGridFit'
    $g.Clear([System.Drawing.Color]::Transparent)
    @{ bmp = $bmp; g = $g }
}
function Save($c, [string]$name) {
    $c.g.Dispose()
    $c.bmp.Save((Join-Path $Out "$name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $c.bmp.Dispose()
    $script:made += $name
}
function NewPen($color, [double]$w) {
    $p = New-Object System.Drawing.Pen($color, [float]$w)
    $p.StartCap = 'Round'; $p.EndCap = 'Round'; $p.LineJoin = 'Round'
    $p
}
function Brush($color) { New-Object System.Drawing.SolidBrush($color) }
function VGrad($rect, $top, $bottom) {
    $r2 = Rect ($rect.X - 1) ($rect.Y - 1) ($rect.Width + 2) ($rect.Height + 2)
    New-Object System.Drawing.Drawing2D.LinearGradientBrush($r2, $top, $bottom, [System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
}

# Rounded rectangle with some corners cut at 45 degrees ($chamfer: any of TL TR BR BL).
function Shape([double]$x, [double]$y, [double]$w, [double]$h, [double]$cut, [double]$r, [string]$chamfer = '') {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $r = [math]::Max($r, 1)
    if ($chamfer -match 'TL') { $p.AddLine((Pt $x ($y + $cut)), (Pt ($x + $cut) $y)) } else { $p.AddArc((Rect $x $y (2 * $r) (2 * $r)), [float]180, [float]90) }
    if ($chamfer -match 'TR') { $p.AddLine((Pt ($x + $w - $cut) $y), (Pt ($x + $w) ($y + $cut))) } else { $p.AddArc((Rect ($x + $w - 2 * $r) $y (2 * $r) (2 * $r)), [float]270, [float]90) }
    if ($chamfer -match 'BR') { $p.AddLine((Pt ($x + $w) ($y + $h - $cut)), (Pt ($x + $w - $cut) ($y + $h))) } else { $p.AddArc((Rect ($x + $w - 2 * $r) ($y + $h - 2 * $r) (2 * $r) (2 * $r)), [float]0, [float]90) }
    if ($chamfer -match 'BL') { $p.AddLine((Pt ($x + $cut) ($y + $h)), (Pt $x ($y + $h - $cut))) } else { $p.AddArc((Rect $x ($y + $h - 2 * $r) (2 * $r) (2 * $r)), [float]90, [float]90) }
    $p.CloseFigure()
    $p
}

# Soft glow around a path: stacked strokes, strongest at the edge, quadratic falloff over $size pixels.
function Glow($g, $path, $color, [int]$size, [double]$strength) {
    if ($size -le 0 -or $strength -le 0) { return }
    $k = 1.1 * $strength / $size
    for ($i = $size; $i -ge 1; $i--) {
        $a = [math]::Min([double]1.0, [double]($k * (1 - ($i - 1) / $size)))
        $pen = NewPen (Col $color.R $color.G $color.B $a) (2 * $i)
        $g.DrawPath($pen, $path); $pen.Dispose()
    }
}

# Removes the alpha of everything $draw paints (white) from the image: anti-aliased cut-outs.
function Cut($c, [scriptblock]$draw) {
    $m = New-Bmp $c.bmp.Width $c.bmp.Height
    & $draw $m.g
    $m.g.Dispose()
    $c.g.Flush()
    $rect = New-Object System.Drawing.Rectangle(0, 0, $c.bmp.Width, $c.bmp.Height)
    $fmt = [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
    $d1 = $c.bmp.LockBits($rect, 'ReadWrite', $fmt); $d2 = $m.bmp.LockBits($rect, 'ReadOnly', $fmt)
    $n = $d1.Stride * $c.bmp.Height
    $a1 = New-Object byte[] $n; $a2 = New-Object byte[] $n
    [Runtime.InteropServices.Marshal]::Copy($d1.Scan0, $a1, 0, $n); [Runtime.InteropServices.Marshal]::Copy($d2.Scan0, $a2, 0, $n)
    for ($i = 3; $i -lt $n; $i += 4) { if ($a2[$i] -ne 0) { $a1[$i] = [byte]([int]$a1[$i] * (255 - [int]$a2[$i]) / 255) } }
    [Runtime.InteropServices.Marshal]::Copy($a1, 0, $d1.Scan0, $n)
    $c.bmp.UnlockBits($d1); $m.bmp.UnlockBits($d2); $m.bmp.Dispose()
}

function Lighten($col, [double]$f) { Col ([int]($col.R + (255 - $col.R) * $f)) ([int]($col.G + (255 - $col.G) * $f)) ([int]($col.B + (255 - $col.B) * $f)) }

# Multiplies every pixel's alpha by $f (the whole sprite, glow included, like an Image colour alpha in Unity).
function Opacity($c, [double]$f) {
    if ($f -ge 0.999) { return }
    $c.g.Flush()
    $rect = New-Object System.Drawing.Rectangle(0, 0, $c.bmp.Width, $c.bmp.Height)
    $d = $c.bmp.LockBits($rect, 'ReadWrite', [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $n = $d.Stride * $c.bmp.Height
    $a = New-Object byte[] $n
    [Runtime.InteropServices.Marshal]::Copy($d.Scan0, $a, 0, $n)
    for ($i = 3; $i -lt $n; $i += 4) { $a[$i] = [byte][math]::Round([int]$a[$i] * $f) }
    [Runtime.InteropServices.Marshal]::Copy($a, 0, $d.Scan0, $n)
    $c.bmp.UnlockBits($d)
}

# ------------------------------------------------------------------ 9-slice skins (160 px, 16 px glow margin, border 48)
# $s: top/bottom fill, border colour, glow colour/strength, sheen, chamfer, corner radius, accent brackets.
function Skin([string]$name, $s) {
    $c = New-Bmp 160 160
    $g = $c.g
    $pad = 16; $w = 160 - 2 * $pad
    $path = Shape $pad $pad $w $w $s.cut $s.r $s.chamfer
    Glow $g $path $s.glow $s.glowSize $s.glowStrength
    # body painted opaque (GDI+ mis-blends translucent layers), with a glassy sheen at the top; see-through comes from Opacity below
    $top = $s.top; $bottom = $s.bottom
    $fill = VGrad (Rect $pad $pad $w $w) $top $bottom
    $blend = New-Object System.Drawing.Drawing2D.ColorBlend(3)
    $blend.Colors = [System.Drawing.Color[]]@((Lighten $top $s.sheen), (Col $top.R $top.G $top.B), (Col $bottom.R $bottom.G $bottom.B))
    $blend.Positions = [float[]]@(0, 0.42, 1)
    $fill.InterpolationColors = $blend
    $g.FillPath($fill, $path); $fill.Dispose()
    $pen = NewPen $s.border $s.borderWidth
    $g.DrawPath($pen, $path); $pen.Dispose()
    if ($s.accent) {
        # brighter brackets along the cut corners, like the sheets' panel corners
        $ap = NewPen $s.accent 4.5
        $L = 14; $cut = $s.cut
        if ($s.chamfer -match 'TL') { $g.DrawLines($ap, [System.Drawing.PointF[]]@((Pt $pad ($pad + $cut + $L)), (Pt $pad ($pad + $cut)), (Pt ($pad + $cut) $pad), (Pt ($pad + $cut + $L) $pad))) }
        if ($s.chamfer -match 'BR') { $e = $pad + $w; $g.DrawLines($ap, [System.Drawing.PointF[]]@((Pt $e ($e - $cut - $L)), (Pt $e ($e - $cut)), (Pt ($e - $cut) $e), (Pt ($e - $cut - $L) $e))) }
        $ap.Dispose()
    }
    Opacity $c ($top.A / 255.0)
    Save $c $name
}
function SkinStyle($top, $bottom, $border, $glow, [double]$glowStrength, [string]$chamfer = 'TL BR', [double]$r = 8, $accent = $null) {
    @{ top = $top; bottom = $bottom; border = $border; borderWidth = 2.5; glow = $glow; glowSize = 15; glowStrength = $glowStrength
       sheen = 0.16; cut = 18; r = $r; chamfer = $chamfer; accent = $accent }
}

# glass panel (navy, cyan edge)
Skin 'Panel' (SkinStyle (Col 18 46 92 0.93) (Col 6 18 42 0.93) (Col 64 196 255) (Col 40 170 255) 0.45 'TL BR' 8 (Col 150 235 255))
# buttons: default / hover (brighter, stronger glow) / action colours / disabled
Skin 'Button_Blue'         (SkinStyle (Col 30 92 176 0.97) (Col 10 38 92 0.97) (Col 84 186 255) (Col 40 150 255) 0.45 'TL BR' 8 (Col 160 225 255))
Skin 'Button_Blue_Hover'   (SkinStyle (Col 44 140 226 0.98) (Col 14 62 134 0.98) (Col 160 240 255) (Col 70 210 255) 0.95 'TL BR' 8 (Col 220 250 255))
Skin 'Button_Green'        (SkinStyle (Col 26 150 82 0.97) (Col 8 72 40 0.97) (Col 80 255 150) (Col 40 230 120) 0.55 'TL BR' 8 (Col 190 255 210))
Skin 'Button_Green_Hover'  (SkinStyle (Col 40 190 104 0.98) (Col 12 100 54 0.98) (Col 170 255 200) (Col 70 255 150) 1.0 'TL BR' 8 (Col 230 255 238))
Skin 'Button_Orange'       (SkinStyle (Col 214 110 26 0.97) (Col 120 46 8 0.97) (Col 255 176 80) (Col 255 130 30) 0.55 'TL BR' 8 (Col 255 220 170))
Skin 'Button_Orange_Hover' (SkinStyle (Col 244 140 40 0.98) (Col 150 62 12 0.98) (Col 255 215 140) (Col 255 160 60) 1.0 'TL BR' 8 (Col 255 240 210))
Skin 'Button_Disabled'     (SkinStyle (Col 42 54 72 0.92) (Col 20 28 40 0.92) (Col 92 112 134) (Col 0 0 0) 0 'TL BR' 8 $null)
# list rows (quiz answers, readiness rows): rounded, no chamfer
Skin 'Option'         (SkinStyle (Col 20 52 98 0.94) (Col 10 30 64 0.94) (Col 58 132 204) (Col 40 140 255) 0.15 '' 14)
Skin 'Option_Hover'   (SkinStyle (Col 28 82 146 0.96) (Col 14 50 100 0.96) (Col 130 228 255) (Col 60 200 255) 0.75 '' 14)
Skin 'Option_Correct' (SkinStyle (Col 30 160 84 0.97) (Col 10 86 44 0.97) (Col 110 255 170) (Col 50 240 130) 0.8 '' 14)
Skin 'Option_Wrong'   (SkinStyle (Col 176 34 46 0.97) (Col 98 12 22 0.97) (Col 255 110 120) (Col 255 60 70) 0.8 '' 14)
Skin 'Row_Ready'      (SkinStyle (Col 14 70 52 0.94) (Col 8 40 32 0.94) (Col 70 220 130) (Col 40 230 120) 0.35 '' 14)
# status popups
Skin 'Popup_Info'     (SkinStyle (Col 16 44 96 0.94) (Col 8 22 52 0.94) (Col 70 150 255) (Col 50 130 255) 0.45 'TL BR' 8 $null)
Skin 'Popup_Success'  (SkinStyle (Col 12 76 44 0.94) (Col 6 38 24 0.94) (Col 70 235 130) (Col 40 230 120) 0.55 'TL BR' 8 $null)
Skin 'Popup_Error'    (SkinStyle (Col 92 16 24 0.94) (Col 46 8 12 0.94) (Col 255 80 90) (Col 255 60 70) 0.55 'TL BR' 8 $null)

# ------------------------------------------------------------------ progress bar (64 x 32 capsules, border 16/14)
$c = New-Bmp 64 32; $p = Shape 2 2 60 28 0 14
$b = Brush (Col 8 22 48 0.95); $c.g.FillPath($b, $p); $b.Dispose()
$pen = NewPen (Col 44 96 150) 2; $c.g.DrawPath($pen, $p); $pen.Dispose(); Save $c 'Bar_Track'
foreach ($v in @(@('Bar_Fill', (Col 140 236 255), (Col 24 156 236)), @('Bar_Fill_Green', (Col 150 255 190), (Col 30 190 96)))) {
    $c = New-Bmp 64 32; $p = Shape 4 5 56 22 0 11
    $b = VGrad (Rect 4 5 56 22) $v[1] $v[2]; $c.g.FillPath($b, $p); $b.Dispose(); Save $c $v[0]
}

# ------------------------------------------------------------------ icon tiles (rounded square wells behind button icons)
foreach ($t in @(@('IconTile_Blue', (Col 50 130 220), (Col 12 46 110), (Col 120 210 255)), @('IconTile_Green', (Col 40 190 100), (Col 10 90 48), (Col 140 255 180)),
                 @('IconTile_Orange', (Col 250 150 50), (Col 140 56 10), (Col 255 210 150)))) {
    $c = New-Bmp 128 128; $p = Shape 10 10 108 108 0 22
    Glow $c.g $p $t[3] 9 0.35
    $b = VGrad (Rect 10 10 108 108) $t[1] $t[2]; $c.g.FillPath($b, $p); $b.Dispose()
    $c.g.SetClip($p); $s = VGrad (Rect 10 10 108 54) (Col 255 255 255 0.22) (Col 255 255 255 0); $c.g.FillRectangle($s, (Rect 10 10 108 54)); $s.Dispose(); $c.g.ResetClip()
    $pen = NewPen $t[3] 3; $c.g.DrawPath($pen, $p); $pen.Dispose()
    Save $c $t[0]
}

# ------------------------------------------------------------------ check boxes, step dots, rings (96 px, 48 px body centred)
function CheckMark($g, [double]$cx, [double]$cy, [double]$s, $color, [double]$w) {
    $pen = NewPen $color $w
    $g.DrawLines($pen, [System.Drawing.PointF[]]@((Pt ($cx - 0.42 * $s) ($cy + 0.02 * $s)), (Pt ($cx - 0.12 * $s) ($cy + 0.32 * $s)), (Pt ($cx + 0.45 * $s) ($cy - 0.3 * $s))))
    $pen.Dispose()
}
$c = New-Bmp 96 96; $p = Shape 24 24 48 48 0 10
$b = Brush (Col 8 24 50 0.9); $c.g.FillPath($b, $p); $b.Dispose()
$pen = NewPen (Col 110 150 190) 3.5; $c.g.DrawPath($pen, $p); $pen.Dispose(); Save $c 'Checkbox_Off'
$c = New-Bmp 96 96; $p = Shape 24 24 48 48 0 10
Glow $c.g $p (Col 50 240 130) 12 0.7
$b = VGrad (Rect 24 24 48 48) (Col 70 220 120) (Col 20 150 70); $c.g.FillPath($b, $p); $b.Dispose()
$pen = NewPen (Col 160 255 190) 2.5; $c.g.DrawPath($pen, $p); $pen.Dispose()
CheckMark $c.g 48 48 40 $White 6.5; Save $c 'Checkbox_On'

$c = New-Bmp 96 96
$b = Brush (Col 8 24 50 0.9); $c.g.FillEllipse($b, (Rect 26 26 44 44)); $b.Dispose()
$pen = NewPen (Col 96 124 156) 3.5; $c.g.DrawEllipse($pen, (Rect 26 26 44 44)); $pen.Dispose(); Save $c 'Step_Pending'
$c = New-Bmp 96 96; $e = New-Object System.Drawing.Drawing2D.GraphicsPath; $e.AddEllipse((Rect 26 26 44 44))
Glow $c.g $e (Col 60 210 255) 18 0.9
$b = VGrad (Rect 26 26 44 44) (Col 150 240 255) (Col 30 170 240); $c.g.FillEllipse($b, (Rect 26 26 44 44)); $b.Dispose()
$b = Brush $White; $c.g.FillEllipse($b, (Rect 40 40 16 16)); $b.Dispose(); Save $c 'Step_Active'
$c = New-Bmp 96 96; $e = New-Object System.Drawing.Drawing2D.GraphicsPath; $e.AddEllipse((Rect 26 26 44 44))
Glow $c.g $e (Col 50 240 130) 10 0.5
$b = VGrad (Rect 26 26 44 44) (Col 80 230 130) (Col 20 150 70); $c.g.FillEllipse($b, (Rect 26 26 44 44)); $b.Dispose()
CheckMark $c.g 48 48 34 $White 6; Save $c 'Step_Done'

# ring with glow (tinted in Unity): countdown and badges
$c = New-Bmp 256 256; $e = New-Object System.Drawing.Drawing2D.GraphicsPath; $e.AddEllipse((Rect 32 32 192 192))
Glow $c.g $e $White 26 0.7
$pen = NewPen $White 9; $c.g.DrawEllipse($pen, (Rect 32 32 192 192)); $pen.Dispose()
$pen = NewPen (Col 255 255 255 0.45) 3; $c.g.DrawEllipse($pen, (Rect 48 48 160 160)); $pen.Dispose(); Save $c 'Ring_Glow'
# soft round light
$c = New-Bmp 128 128; $e = New-Object System.Drawing.Drawing2D.GraphicsPath; $e.AddEllipse((Rect 0 0 128 128))
$pg = New-Object System.Drawing.Drawing2D.PathGradientBrush($e); $pg.CenterColor = $White; $pg.SurroundColors = [System.Drawing.Color[]]@((Col 255 255 255 0))
$c.g.FillEllipse($pg, (Rect 0 0 128 128)); $pg.Dispose(); Save $c 'Disc_Soft'
# answer letter circle
$c = New-Bmp 96 96
$b = Brush (Col 255 255 255 0.12); $c.g.FillEllipse($b, (Rect 14 14 68 68)); $b.Dispose()
$pen = NewPen $White 4; $c.g.DrawEllipse($pen, (Rect 14 14 68 68)); $pen.Dispose(); Save $c 'Letter_Circle'
# header underline: bright at the left, fading out to the right
$c = New-Bmp 512 16
$lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush((Rect 0 0 513 16), (Col 90 210 255 0.5), (Col 90 210 255 0), [System.Drawing.Drawing2D.LinearGradientMode]::Horizontal)
$c.g.FillRectangle($lg, (Rect 0 4 512 8)); $lg.Dispose()
$lg = New-Object System.Drawing.Drawing2D.LinearGradientBrush((Rect 0 0 513 16), (Col 160 235 255 1), (Col 160 235 255 0), [System.Drawing.Drawing2D.LinearGradientMode]::Horizontal)
$c.g.FillRectangle($lg, (Rect 0 7 512 2.5)); $lg.Dispose()
$b = Brush (Col 200 245 255); $c.g.FillRectangle($b, (Rect 0 5 36 6)); $b.Dispose(); Save $c 'Divider'

# ------------------------------------------------------------------ icons (128 px, white, tinted in Unity)
function Icon([string]$name, [double]$angle, [scriptblock]$draw, [scriptblock]$cut = $null) {
    $c = New-Bmp 128 128
    if ($angle -ne 0) { $c.g.TranslateTransform(64, 64); $c.g.RotateTransform([float]$angle); $c.g.TranslateTransform(-64, -64) }
    & $draw $c.g
    if ($cut) {
        $ang = $angle
        Cut $c { param($m) if ($ang -ne 0) { $m.TranslateTransform(64, 64); $m.RotateTransform([float]$ang); $m.TranslateTransform(-64, -64) }; & $cut $m }
    }
    Save $c $name
}
function Fill($g, $path) { $b = Brush $White; $g.FillPath($b, $path); $b.Dispose() }
function Poly([System.Drawing.PointF[]]$pts) { $p = New-Object System.Drawing.Drawing2D.GraphicsPath; $p.AddPolygon($pts); $p }
function Txt($g, [string]$s, [double]$size, $rect) {
    $f = New-Object System.Drawing.Font('Segoe UI Black', [float]$size, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $sf = New-Object System.Drawing.StringFormat; $sf.Alignment = 'Center'; $sf.LineAlignment = 'Center'
    $b = Brush $White; $g.DrawString($s, $f, $b, $rect, $sf); $b.Dispose(); $f.Dispose()
}

Icon 'Icon_Book' 0 {
    param($g)
    foreach ($m in 1, -1) {
        $mx = { param($v) if ($m -eq 1) { $v } else { 128 - $v } }
        $p = New-Object System.Drawing.Drawing2D.GraphicsPath
        $p.AddBezier((Pt (& $mx 10) 30), (Pt (& $mx 28) 22), (Pt (& $mx 46) 26), (Pt (& $mx 60) 38))
        $p.AddLine((Pt (& $mx 60) 38), (Pt (& $mx 60) 106))
        $p.AddBezier((Pt (& $mx 60) 106), (Pt (& $mx 46) 94), (Pt (& $mx 28) 90), (Pt (& $mx 10) 98))
        $p.CloseFigure(); Fill $g $p
    }
} {
    param($g)
    $pen = NewPen $White 4.5
    foreach ($y in 50, 64, 78) { $g.DrawLine($pen, (Pt 20 ($y - 4)), (Pt 50 ($y + 2))); $g.DrawLine($pen, (Pt 108 ($y - 4)), (Pt 78 ($y + 2))) }
    $pen.Dispose()
}

Icon 'Icon_Rocket' 45 {
    param($g)
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddBezier((Pt 64 6), (Pt 84 24), (Pt 86 50), (Pt 82 84)); $p.AddLine((Pt 82 84), (Pt 46 84)); $p.AddBezier((Pt 46 84), (Pt 42 50), (Pt 44 24), (Pt 64 6))
    $p.CloseFigure(); Fill $g $p
    Fill $g (Poly @((Pt 46 58), (Pt 28 80), (Pt 28 98), (Pt 46 86)))
    Fill $g (Poly @((Pt 82 58), (Pt 100 80), (Pt 100 98), (Pt 82 86)))
    $b = Brush $White; $g.FillRectangle($b, (Rect 54 84 20 8)); $b.Dispose()
    $f = New-Object System.Drawing.Drawing2D.GraphicsPath
    $f.AddBezier((Pt 52 96), (Pt 52 108), (Pt 60 114), (Pt 64 126)); $f.AddBezier((Pt 64 126), (Pt 68 114), (Pt 76 108), (Pt 76 96)); $f.CloseFigure()
    $b = Brush (Col 255 255 255 0.8); $g.FillPath($b, $f); $b.Dispose()
} {
    param($g)
    $b = Brush $White; $g.FillEllipse($b, (Rect 55 34 18 18)); $b.Dispose()
}

Icon 'Icon_Satellite' -35 {
    param($g)
    $b = Brush $White
    $g.FillPath($b, (Shape 50 44 28 40 0 5))
    $g.FillRectangle($b, (Rect 8 52 36 24)); $g.FillRectangle($b, (Rect 84 52 36 24))
    $pen = NewPen $White 5; $g.DrawLine($pen, (Pt 44 64), (Pt 50 64)); $g.DrawLine($pen, (Pt 78 64), (Pt 84 64)); $g.DrawLine($pen, (Pt 64 44), (Pt 64 34)); $pen.Dispose()
    $d = New-Object System.Drawing.Drawing2D.GraphicsPath; $d.AddArc((Rect 50 18 28 22), [float]0, [float]180); $d.CloseFigure(); $g.FillPath($b, $d)
    $g.FillEllipse($b, (Rect 58 88 12 12))
    $b.Dispose()
} {
    param($g)
    $pen = NewPen $White 3
    foreach ($x0 in 8, 84) { $g.DrawLine($pen, (Pt ($x0 + 12) 52), (Pt ($x0 + 12) 76)); $g.DrawLine($pen, (Pt ($x0 + 24) 52), (Pt ($x0 + 24) 76)); $g.DrawLine($pen, (Pt $x0 64), (Pt ($x0 + 36) 64)) }
    $pen.Dispose()
}

Icon 'Icon_Quiz' 0 {
    param($g)
    $pen = NewPen $White 8; $g.DrawPath($pen, (Shape 12 14 104 82 0 20)); $pen.Dispose()
    Fill $g (Poly @((Pt 34 92), (Pt 28 118), (Pt 58 94)))
    Txt $g '?' 64 (Rect 12 12 104 84)
}

Icon 'Icon_Check' 0 { param($g) CheckMark $g 64 64 96 $White 15 }
Icon 'Icon_Cross' 0 { param($g) $pen = NewPen $White 15; $g.DrawLine($pen, (Pt 32 32), (Pt 96 96)); $g.DrawLine($pen, (Pt 96 32), (Pt 32 96)); $pen.Dispose() }
Icon 'Icon_Chevron' 0 { param($g) $pen = NewPen $White 15; $g.DrawLines($pen, [System.Drawing.PointF[]]@((Pt 46 22), (Pt 88 64), (Pt 46 106))); $pen.Dispose() }

Icon 'Icon_Info' 0 { param($g) $b = Brush $White; $g.FillEllipse($b, (Rect 8 8 112 112)); $b.Dispose() } {
    param($g) $b = Brush $White; $g.FillEllipse($b, (Rect 55 26 18 18)); $g.FillPath($b, (Shape 56 52 16 48 0 5)); $b.Dispose()
}
Icon 'Icon_Warning' 0 {
    param($g)
    $t = Poly @((Pt 64 14), (Pt 116 108), (Pt 12 108))
    $pen = NewPen $White 12; $g.DrawPath($pen, $t); $pen.Dispose(); Fill $g $t
} {
    param($g) $b = Brush $White; $g.FillPath($b, (Shape 57 40 14 42 0 5)); $g.FillEllipse($b, (Rect 56 88 16 16)); $b.Dispose()
}

Icon 'Icon_Target' 0 {
    param($g)
    $pen = NewPen $White 9; $g.DrawEllipse($pen, (Rect 14 14 100 100)); $g.DrawEllipse($pen, (Rect 36 36 56 56)); $pen.Dispose()
    $b = Brush $White; $g.FillEllipse($b, (Rect 54 54 20 20)); $b.Dispose()
}

Icon 'Icon_Chart' 0 { param($g) $b = Brush $White; $g.FillPath($b, (Shape 14 74 24 40 0 4)); $g.FillPath($b, (Shape 52 48 24 66 0 4)); $g.FillPath($b, (Shape 90 18 24 96 0 4)); $b.Dispose() }

Icon 'Icon_Orbit' 0 {
    param($g)
    $g.TranslateTransform(64, 64); $g.RotateTransform([float]-22); $g.TranslateTransform(-64, -64)
    $pen = NewPen $White 6; $g.DrawEllipse($pen, (Rect 6 44 116 40)); $pen.Dispose()
    $g.ResetTransform()
    $b = Brush $White; $g.FillEllipse($b, (Rect 104 30 16 16)); $b.Dispose()
} {
    param($g) $b = Brush $White; $g.FillEllipse($b, (Rect 34 34 60 60)); $b.Dispose()
}
# planet drawn after the cut so the ring passes behind it
$c = New-Bmp 128 128
$src = [System.Drawing.Image]::FromFile((Join-Path $Out 'Icon_Orbit.png')); $c.g.DrawImage($src, 0, 0, 128, 128); $src.Dispose()
$b = Brush $White; $c.g.FillEllipse($b, (Rect 40 40 48 48)); $b.Dispose()
$c.g.TranslateTransform(64, 64); $c.g.RotateTransform([float]-22); $c.g.TranslateTransform(-64, -64)
$c.g.SetClip((Rect 0 64 128 64)); $pen = NewPen $White 6; $c.g.DrawEllipse($pen, (Rect 6 44 116 40)); $pen.Dispose()
$script:made = @($script:made | Where-Object { $_ -ne 'Icon_Orbit' })
Save $c 'Icon_Orbit'

Icon 'Icon_Layers' 0 {
    param($g)
    Fill $g (Poly @((Pt 64 14), (Pt 114 40), (Pt 64 66), (Pt 14 40)))
    $pen = NewPen $White 9; $g.DrawLines($pen, [System.Drawing.PointF[]]@((Pt 16 64), (Pt 64 88), (Pt 112 64))); $g.DrawLines($pen, [System.Drawing.PointF[]]@((Pt 16 88), (Pt 64 112), (Pt 112 88))); $pen.Dispose()
}
Icon 'Icon_Gauge' 0 {
    param($g)
    $pen = NewPen $White 10; $g.DrawArc($pen, (Rect 12 18 104 104), [float]160, [float]220); $pen.Dispose()
    $pen = NewPen $White 5
    foreach ($a in 180, 225, 270, 315, 360) { $r = $a * [math]::PI / 180; $g.DrawLine($pen, (Pt (64 + 36 * [math]::Cos($r)) (70 + 36 * [math]::Sin($r))), (Pt (64 + 28 * [math]::Cos($r)) (70 + 28 * [math]::Sin($r)))) }
    $pen.Dispose()
    $pen = NewPen $White 8; $g.DrawLine($pen, (Pt 64 70), (Pt 90 42)); $pen.Dispose()
    $b = Brush $White; $g.FillEllipse($b, (Rect 54 60 20 20)); $b.Dispose()
}
Icon 'Icon_Checklist' 0 {
    param($g)
    foreach ($y in 28, 64, 100) {
        CheckMark $g 26 $y 26 $White 7
        $pen = NewPen $White 10; $g.DrawLine($pen, (Pt 52 $y), (Pt 114 $y)); $pen.Dispose()
    }
}
Icon 'Icon_Drop' 0 {
    param($g)
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddBezier((Pt 64 8), (Pt 76 30), (Pt 102 54), (Pt 102 80)); $p.AddArc((Rect 26 42 76 76), [float]0, [float]180); $p.AddBezier((Pt 26 80), (Pt 26 54), (Pt 52 30), (Pt 64 8))
    $p.CloseFigure(); Fill $g $p
} {
    param($g) $pen = NewPen $White 6; $g.DrawArc($pen, (Rect 40 58 46 46), [float]100, [float]70); $pen.Dispose()
}
Icon 'Icon_Cap' 0 {
    param($g)
    Fill $g (Poly @((Pt 64 22), (Pt 120 46), (Pt 64 70), (Pt 8 46)))
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p.AddLine((Pt 32 60), (Pt 32 86)); $p.AddBezier((Pt 32 86), (Pt 48 100), (Pt 80 100), (Pt 96 86)); $p.AddLine((Pt 96 86), (Pt 96 60)); $p.AddLine((Pt 96 60), (Pt 64 74)); $p.CloseFigure(); Fill $g $p
    $pen = NewPen $White 5; $g.DrawLine($pen, (Pt 108 52), (Pt 108 88)); $pen.Dispose()
    $b = Brush $White; $g.FillEllipse($b, (Rect 101 86 14 14)); $b.Dispose()
}

# ------------------------------------------------------------------ preview sheet (written next to this script, not imported)
$cols = 8; $cell = 190
$rows = [math]::Ceiling($script:made.Count / $cols)
$pv = New-Bmp ($cols * $cell) ($rows * $cell)
$bg = VGrad (Rect 0 0 ($cols * $cell) ($rows * $cell)) (Col 10 22 44) (Col 4 10 24); $pv.g.FillRectangle($bg, (Rect 0 0 ($cols * $cell) ($rows * $cell))); $bg.Dispose()
$font = New-Object System.Drawing.Font('Segoe UI', [float]12, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
$lb = Brush (Col 170 200 230)
for ($i = 0; $i -lt $script:made.Count; $i++) {
    $n = $script:made[$i]; $x = ($i % $cols) * $cell; $y = [math]::Floor($i / $cols) * $cell
    $img = [System.Drawing.Image]::FromFile((Join-Path $Out "$n.png"))
    $s = [math]::Min(150 / $img.Width, 150 / $img.Height)
    $w = $img.Width * $s; $h = $img.Height * $s
    $pv.g.DrawImage($img, (Rect ($x + ($cell - $w) / 2) ($y + 10 + (150 - $h) / 2) $w $h))
    $img.Dispose()
    $pv.g.DrawString($n, $font, $lb, (Pt ($x + 8) ($y + $cell - 24)))
}
$pv.g.Dispose(); $pv.bmp.Save((Join-Path $PSScriptRoot 'Preview.png'), [System.Drawing.Imaging.ImageFormat]::Png); $pv.bmp.Dispose()
"made $($script:made.Count) sprites in $Out"
