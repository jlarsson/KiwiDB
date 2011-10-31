#derived from https://github.com/hibernating-rhinos/rhino-esb/blob/master/psake_ext.ps1 by Ayende Rahien
function Get-Git-Commit
{
	$gitLog = git log --oneline -1
	return $gitLog.Split(' ')[0]
}

function Get-Version-From-Git-Tag
{
  $gitTag = git describe --tags --abbrev=0
  return $gitTag.Replace("v", "")
}

function Create-Project-Description
{
	param(
		[string]$base_dir = $(throw "base_dir is a required parameter."),
		[string]$project_name = $(throw "project_name is a required parameter."),
		[string]$id = $(throw "id is a required parameter."),
		[string]$author = $(throw "author is a required parameter."),
		[string]$description = $(throw "description is a required parameter."),
		[string]$copyright = $(throw "copyright is a required parameter."),
		[string]$clsCompliant = "true"
	)
	return @{
		clsCompliant = $clsCompliant; `
		project_name = $project_name; `
		project_dir = join-path $base_dir $project_name; `
		project_file = join-path (join-path $base_dir $project_name) "$project_name.csproj"; `
		nuspec_file = join-path (join-path $base_dir $project_name) "$project_name.nuspec"; `
		assemblyinfo_file = join-path $base_dir "$project_name\Properties\AssemblyInfo.cs"; `
		version = (Get-Version-From-Git-Tag); `
		commit = (Get-Git-Commit); `
		id = $id; `
		author = $author; `
		title = "$project_name $version"; `
		description = $description; `
		copyright = $copyright; `
	}
}

function Ensure-File-Directory {
	param([string]$file = $(throw "file is a required parameter."))
	$dir = [System.IO.Path]::GetDirectoryName($file)
	Ensure-Directory $dir
}

function Ensure-Directory {
	param ([string]$dir = $(throw "dir is a required parameter."))
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
}
function Print-Project-Description {
	param($project)
	Write-Host project_name = $project.project_name
	Write-Host project_dir = $project.project_dir
	Write-Host project_file = $project.project_file
	Write-Host assemblyinfo_file = $project.assemblyinfo_file
	Write-Host id = $project.id
	Write-Host version = $project.version
	Write-Host author = $project.author
	Write-Host title = $project.title
	Write-Host description = $project.description
	Write-Host copyright = $project.copyright
}

function Generate-Assembly-Info
{
	param($project)

	$asmInfo = "using System;
using System.Reflection;
using System.Runtime.InteropServices;
[assembly: CLSCompliantAttribute($($project.clsCompliant))]
[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyTitleAttribute(""$($project.title)"")]
[assembly: AssemblyDescriptionAttribute(""$($project.description)"")]
[assembly: AssemblyCompanyAttribute(""$($project.author)"")]
[assembly: AssemblyProductAttribute(""$($project.id) $($project.version)"")]
[assembly: AssemblyCopyrightAttribute(""$($project.copyright)"")]
[assembly: AssemblyVersionAttribute(""$($project.version)"")]
[assembly: AssemblyInformationalVersionAttribute(""$($project.version) / $($project.commit)"")]
[assembly: AssemblyFileVersionAttribute(""$($project.version)"")]
[assembly: AssemblyDelaySignAttribute(false)]
"
	$file = $project.assemblyinfo_file
	Ensure-File-Directory $file
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
}