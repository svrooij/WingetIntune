$pesterConfig = [PesterConfiguration]@{
  Output = @{
    Verbosity = "Normal"
  }
  OutputFormat = "NUnitXML"
  TestResult = @{
    Enabled = $true
    Path = "TestResults.xml"
    Format = "NUnitXML"
  }
  Run = @{
    Path = "./tests/WinTuner.Cmdlets.Tests"
    Exit = $true
  }
  Should = @{
    ErrorAction = "Continue"
  }
}

Invoke-Pester -Configuration $pesterConfig