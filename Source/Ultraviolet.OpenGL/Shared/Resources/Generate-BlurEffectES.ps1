﻿function Generate-BlurEffectES {
	param(
		[Parameter(Mandatory=$true)]
		[System.Int32] $Radius
	)

	$Shader = New-Object System.Collections.Generic.List[System.String]
	$Shader.Add("#includeres `"Ultraviolet.OpenGL.Resources.HeaderES.fragh`" executing")
	$Shader.Add("")
	$Shader.Add("uniform sampler2D Texture;")
	$Shader.Add("")
	$Shader.Add("uniform float Resolution;")
	$Shader.Add("uniform float Mix;")
	$Shader.Add("uniform vec2 Direction;")
	$Shader.Add("")
	$Shader.Add("DECLARE_INPUT_COLOR;`t// vColor")
	$Shader.Add("DECLARE_INPUT_TEXCOORD;`t// vTextureCoordinate")
	$Shader.Add("")
	$Shader.Add("DECLARE_OUTPUT_COLOR;`t// fColor")
	$Shader.Add("")
	$Shader.Add("void main()")
	$Shader.Add("{")
	$Shader.Add("	// Modified from http://callumhay.blogspot.com/2010/09/gaussian-blur-shader-glsl.html")
	$Shader.Add("	")
	$Shader.Add("	float step = 1.0 / Resolution;")
	$Shader.Add("	")

	$IncGaussX = 1.0 / ([System.Math]::Sqrt(2.0 * [System.Math]::PI) * $Radius)
	$IncGaussY = [System.Math]::Exp(-0.5 / ($Radius * $Radius))
	$IncGaussZ = $IncGaussY * $IncGaussY
	$CoefficientSum = $IncGaussX

	$Shader.Add("	vec4 avgValue = vec4(0.0, 0.0, 0.0, 0.0);")
	$Shader.Add("	avgValue += texture(Texture, vTextureCoordinate.xy) * $IncGaussX;")

	$IncGaussX *= $IncGaussY;
	$IncGaussY *= $IncGaussZ;

	for ($I = 1; $I -le $Radius; $I++) {
		$Shader.Add("	avgValue += texture(Texture, vTextureCoordinate.xy - (float($I) * step * Direction)) * $IncGaussX;")
		$Shader.Add("	avgValue += texture(Texture, vTextureCoordinate.xy + (float($I) * step * Direction)) * $IncGaussX;")
		$CoefficientSum += 2.0 * $IncGaussX;
		$IncGaussX *= $IncGaussY;
		$IncGaussY *= $IncGaussZ;
	}	

	$Shader.Add("	")
	$Shader.Add("	vec4 blur = avgValue / $CoefficientSum;")
	$Shader.Add("	vec4 outBlurred = blur * vColor.a;")
	$Shader.Add("	vec4 outColored = vColor * blur.a;")
	$Shader.Add("	");
	$Shader.Add("	fColor = mix(outBlurred, outColored, Mix);")
	$Shader.Add("}")

	Out-File -FilePath "BlurEffectRadius$($Radius)ES.frag" -InputObject $Shader
}

for ($R = 1; $R -le 9; $R += 2) {
	Write-Host "Generating radius $R..."
	Generate-BlurEffectES -Radius $R
}