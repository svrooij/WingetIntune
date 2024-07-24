$pesterConfig = [PesterConfiguration]@{
  Output = @{
    Verbosity = "Detailed"
    CIFormat = "Auto"
    StackTraceVerbosity = "FirstLine"
  }
  TestResult = @{
    Enabled = $true
    OutputPath = "TestResults.xml"
    OutputFormat = "JUnitXml"
  }
  Run = @{
    Path = "./tests/WinTuner.Proxy.Tests"
    Exit = $true
  }
  Should = @{
    ErrorAction = "Continue"
  }
}

Invoke-Pester -Configuration $pesterConfig