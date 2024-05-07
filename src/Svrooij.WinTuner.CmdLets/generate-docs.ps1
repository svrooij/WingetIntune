$xmlDocsPath = "${PSScriptRoot}\bin\Release\net6.0\Svrooij.WinTuner.CmdLets.xml"
$docsFolder = "docs"

Write-Output "Building the project to get the latest XML documentation file"
# Start Process to build the project
$buildOutput = & dotnet build "${PSScriptRoot}" -c Release -v quiet

# Check if the build succeeded by looking for the string "Build succeeded."
if ($buildOutput -match "Build succeeded.") {
    Write-Output "Project build succeeded"
}
else {
    Write-Output "Build failed"
    Write-Output $buildOutput
    exit
}

Write-Output "Generating docs"

New-MarkdownHelp -Module "Svrooij.WinTuner.CmdLets" -OutputFolder "${PSScriptRoot}\docs" -WithModulePage

Write-Output "Generating docs from XML file $xmlDocsPath"

# Load the XML documentation file
[xml]$xmlDocs = Get-Content $xmlDocsPath

$assemblyName = $xmlDocs.doc.assembly.name
Write-Debug "Updating docs for Assembly: $assemblyName"

$members = $xmlDocs.doc.members.member

# Iterate over all <member> objects
foreach ($member in $members) {
    # member looks like this:
    # <member name="T:Your.Namespace.ClassNameForPsCmdLet">
    #   <summary>
    #   <para type="synopsis">synopsis here</para>
    #   <para type="description">PsCmdLet description here</para>
    #   <para type="link" uri="https://wintuner.app/docs/related/content-prep-tool">Documentation</para> 
    #   </summary>
    #   <example>
    #   <para type="description">Sample description here</para>
    #   <code>Sample Code here</code>
    #   </example>
    # </member>
    # Extract the name of the member
    $name = $member.Attributes[0].'#text'

    if ($name.startsWith("T:" + $assemblyName)) {
		$name = $name.substring(2 + $assemblyName.length + 1)
        if ($name.contains(".")) {
			$name = $name.substring($name.lastIndexOf(".") + 1)
		}


        # name NewIntuneWinPackage
        Write-Output "Try to update markdown file for: $name"

        # Extract the synopsis
        $synopsis = $member.summary.'para' | Where-Object { $_.type -eq 'synopsis' } | Select-Object -ExpandProperty '#text'
        Write-Debug "Synopsis: $synopsis"

        # Extract the description
        $description = $member.summary.'para' | Where-Object { $_.type -eq 'description' } | Select-Object -ExpandProperty '#text'
        Write-Debug "Description: $description"

        # Extract the link
        $link = $member.summary.'para' | Where-Object { $_.type -eq 'link' } | Select-Object -ExpandProperty 'uri'
        Write-Debug "Link: $link"


        # Extract the example
        $exampleDescription = $member.example.'para'| Where-Object { $_.type -eq 'description' } | Select-Object -ExpandProperty '#text'
        $exampleCode = $member.example.'code'

        Write-Debug "Example Description: $exampleDescription"
        Write-Debug "Example Code: $exampleCode"

        # Create the MD file name by putting a - only before the second capital letter, so NewIntuneWinPackage becomes New-IntuneWinPackage
        $index = $name.IndexOfAny([char[]]"ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), 1)
        $mdFile = $name.Substring(0, $index) + "-" + $name.Substring($index) + ".md"
        Write-Debug "MD Filename: $mdFile"

        # Check if the $mdFile exists in the docs folder
        $mdFilePath = Join-Path "${PSScriptRoot}\$docsFolder" $mdFile
        if (Test-Path $mdFilePath) {
            # load the existing file
            $mdFileContent = Get-Content $mdFilePath

            # Replace the synopsis placeholder '{{ Fill in the Synopsis }}' with the actual synopsis
            $mdFileContent = $mdFileContent.Replace('{{ Fill in the Synopsis }}', $synopsis)

            # Replace the description placeholder '{{ Fill in the Description }}' with the actual description
            $mdFileContent = $mdFileContent.Replace('{{ Fill in the Description }}', $description)

            # Replace the example description placeholder '{{ Add example description here }}' with the actual example description
            $mdFileContent = $mdFileContent.Replace('{{ Add example description here }}', $exampleDescription)

            # Replace the example code placeholder '{{ Add example code here }}' with the actual example code
            $mdFileContent = $mdFileContent.Replace('{{ Add example code here }}', $exampleCode)

            # Add the link to the documentation
            # Check if there is a line that starts with 'online version:' without a link behind it
            if ($mdFileContent.Contains('online version:') -and !$mdFileContent.Contains('online version: http'))
            {
                $mdFileContent = $mdFileContent.Replace('online version:', 'online version: ' + $link)
            }
            

            # Write the updated file back to disk
            $mdFileContent | Set-Content $mdFilePath
        }
        else {
            #Write-Warning "File $mdFilePath does not exist"
        }
	}
}

Write-Output "Done updating markdown files with XML documentation"

Update-MarkdownHelpModule -Path "${PSScriptRoot}\$docsFolder" -RefreshModulePage

New-ExternalHelp -Path "${PSScriptRoot}\$docsFolder" -OutputPath "${PSScriptRoot}" -Force
