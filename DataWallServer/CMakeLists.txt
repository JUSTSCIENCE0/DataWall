cmake_minimum_required(VERSION 3.8)

project(DataWallServer LANGUAGES CSharp)
include(CSharpUtilities)

set(SOURCES App.config 
            Client.cs
			DataWallServer_Main.cs
			DataWallServer_Main.Designer.cs
			DataWallServer_Main.resx
			DBEntities.cs
			Logger.cs
			Program.cs
			Server.cs 
            Properties/AssemblyInfo.cs
			Properties/Resources.Designer.cs
			Properties/Resources.resx
			Properties/Settings.Designer.cs
			Properties/Settings.settings
)

add_executable(DataWallServer ${SOURCES})

set_property(TARGET DataWallServer PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.7.2")
set(CMAKE_CSharp_FLAGS "/langversion:latest")

csharp_set_windows_forms_properties(
    DataWallServer_Main.cs
    DataWallServer_Main.Designer.cs
	DataWallServer_Main.resx
    Program.cs
    Properties/AssemblyInfo.cs
    Properties/Resources.Designer.cs
    Properties/Resources.resx
    Properties/Settings.Designer.cs
    Properties/Settings.settings
)

set_property(TARGET DataWallServer PROPERTY VS_DOTNET_REFERENCES
    "Microsoft.CSharp"
    "System"
    "System.Core"
    "System.Data"
	"System.Data.DataSetExtensions"
	"System.Deployment"
    "System.Drawing"
    "System.Windows.Forms"
	"System.Xml"
	"System.Xml.Linq"
	"MySql.Data"
)