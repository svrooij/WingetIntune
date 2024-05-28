
Describe 'Search-WtWinGetPackage' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Search-WtWinGetPackage'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }

    It 'Should return a list of packages' {
        $packages = Search-WtWinGetPackage -Name 'Firef'
        $packages | Should -Not -BeNullOrEmpty
    }

    It 'Should return a list of packages with a specific name' {
        $packages = Search-WtWinGetPackage -Name 'Firef'
        $packages | ForEach-Object {
            $_.PackageId | Should -Match 'Firef'
        }
    }
}