Describe 'Unprotect-IntuneWinPackage' {
    It 'Should be available' {
        $cmdlet = Get-Command -Name 'Unprotect-IntuneWinPackage'
        $cmdlet.CommandType | Should -Be 'Cmdlet'
    }
}