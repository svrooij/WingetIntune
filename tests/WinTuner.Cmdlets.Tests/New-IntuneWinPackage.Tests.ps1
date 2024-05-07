
Describe 'New-IntuneWinPackage' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'New-IntuneWinPackage'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}