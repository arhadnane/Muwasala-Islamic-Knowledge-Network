#!/usr/bin/env python3
"""
Simple test script to verify MCP server functionality
"""
import json
import subprocess
import sys
import time

def send_request(request):
    """Send a JSON-RPC request to the MCP server"""
    # Create the MCP server process
    process = subprocess.Popen(
        ["dotnet", "run", "--project", "Muwasala.MCP.csproj"],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True
    )
    
    # Send the request
    request_json = json.dumps(request) + "\n"
    stdout, stderr = process.communicate(input=request_json)
    
    if stderr:
        print(f"Error: {stderr}", file=sys.stderr)
    
    if stdout:
        try:
            response = json.loads(stdout.strip())
            return response
        except json.JSONDecodeError:
            print(f"Invalid JSON response: {stdout}")
            return None
    
    return None

def test_initialize():
    """Test the initialize request"""
    request = {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "initialize",
        "params": {
            "protocolVersion": "2024-11-05",
            "capabilities": {
                "roots": {
                    "listChanged": True
                },
                "sampling": {}
            },
            "clientInfo": {
                "name": "test-client",
                "version": "1.0.0"
            }
        }
    }
    
    print("Testing initialize...")
    response = send_request(request)
    if response:
        print(f"Initialize response: {json.dumps(response, indent=2)}")
    else:
        print("No response received")

def test_list_tools():
    """Test listing tools"""
    request = {
        "jsonrpc": "2.0",
        "id": 2,
        "method": "tools/list"
    }
    
    print("\nTesting tools/list...")
    response = send_request(request)
    if response:
        print(f"Tools list response: {json.dumps(response, indent=2)}")
    else:
        print("No response received")

def test_query_database():
    """Test database query"""
    request = {
        "jsonrpc": "2.0",
        "id": 3,
        "method": "tools/call",
        "params": {
            "name": "list_tables",
            "arguments": {}
        }
    }
    
    print("\nTesting list_tables tool...")
    response = send_request(request)
    if response:
        print(f"List tables response: {json.dumps(response, indent=2)}")
    else:
        print("No response received")

if __name__ == "__main__":
    test_initialize()
    test_list_tools()
    test_query_database()
