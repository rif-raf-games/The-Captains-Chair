// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
using UnityEditor;

namespace AmplifyShaderEditor
{
	public class TemplateMenuItems
	{
		[MenuItem( "Assets/Create/Amplify Shader/Impostors/Bake/Legacy", false, 85 )]
		public static void ApplyTemplateImpostorsBakeLegacy()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "f53051a8190f7044fa936bd7fbe116c1" );
		}
		[MenuItem( "Assets/Create/Amplify Shader/Impostors/Runtime/Standard Legacy", false, 85 )]
		public static void ApplyTemplateImpostorsRuntimeStandardLegacy()
		{
			AmplifyShaderEditorWindow.CreateConfirmationTemplateShader( "30a8e337ed84177439ca24b6a5c97cd1" );
		}
	}
}
