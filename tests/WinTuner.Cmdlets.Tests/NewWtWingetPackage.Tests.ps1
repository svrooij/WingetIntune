Describe 'New-WtWingetPackage' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'New-WtWingetPackage'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}