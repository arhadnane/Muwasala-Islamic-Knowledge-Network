# Test MCP Server functionality
# This script tests the MCP server by sending JSON-RPC requests

Write-Host "Testing MCP Server..." -ForegroundColor Green

# Test 1: Initialize request
$initRequest = @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{
            roots = @{
                listChanged = $true
            }
            sampling = @{}
        }
        clientInfo = @{
            name = "test-client"
            version = "1.0.0"
        }
    }
} | ConvertTo-Json -Depth 10

Write-Host "Sending initialize request..." -ForegroundColor Yellow
Write-Host $initRequest

# Test 2: List tools request
$toolsRequest = @{
    jsonrpc = "2.0"
    id = 2
    method = "tools/list"
    params = @{}
} | ConvertTo-Json -Depth 10

Write-Host "`nSending tools/list request..." -ForegroundColor Yellow
Write-Host $toolsRequest

# Test 3: List tables tool call
$listTablesRequest = @{
    jsonrpc = "2.0"
    id = 3
    method = "tools/call"
    params = @{
        name = "list_tables"
        arguments = @{}
    }
} | ConvertTo-Json -Depth 10

Write-Host "`nSending list_tables tool call..." -ForegroundColor Yellow
Write-Host $listTablesRequest

Write-Host "`nTo test manually, run the MCP server and paste these JSON requests." -ForegroundColor Cyan
