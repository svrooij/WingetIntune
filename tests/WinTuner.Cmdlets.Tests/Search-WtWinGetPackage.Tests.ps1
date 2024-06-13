
Describe 'Search-WtWinGetPackage' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Search-WtWinGetPackage'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }

    It 'Should return a list of packages' {
        $packages = Search-WtWinGetPackage 'Firef'
        $packages | Should -Not -BeNullOrEmpty
    }

    It 'Should return a list of packages with a specific name' {
        $packages = Search-WtWinGetPackage 'Firef'
        $packages | ForEach-Object {
            $_.PackageId | Should -Match '(?i)Firef'
        }
    }

    It 'Should allow filtering outputs' {
        $packages = Search-WtWinGetPackage 'Microsoft.Visual'
        $packageFiltered = Search-WtWinGetPackage 'Microsoft.Visual'  | Where-Object { $_.PackageId -eq 'Microsoft.VisualStudioCode' }

        $packages.Count | Should -BeGreaterThan $packageFiltered.Count
        $packageFiltered.Count | Should -Be 1
    }
}