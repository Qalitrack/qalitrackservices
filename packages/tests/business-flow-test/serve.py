#!/usr/bin/env python3
"""
Simple HTTP server to serve the QaliTrack Business Flow Test Interface
Usage: python3 serve.py [port]
Default port: 8080
"""

import http.server
import socketserver
import sys
import os
import webbrowser
from pathlib import Path

def serve_interface(port=8080):
    """Serve the business flow test interface on the specified port"""
    
    # Change to the directory containing this script
    script_dir = Path(__file__).parent.absolute()
    os.chdir(script_dir)
    
    # Create request handler
    Handler = http.server.SimpleHTTPRequestHandler
    
    # Custom handler to set proper MIME types
    class CustomHandler(Handler):
        def end_headers(self):
            self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
            self.send_header('Pragma', 'no-cache')
            self.send_header('Expires', '0')
            super().end_headers()
    
    try:
        with socketserver.TCPServer(("", port), CustomHandler) as httpd:
            print(f"🏭 QaliTrack Business Flow Test Interface")
            print(f"📡 Server started at: http://localhost:{port}")
            print(f"📁 Serving from: {script_dir}")
            print(f"🌐 Open http://localhost:{port} in your web browser")
            print(f"⏹️  Press Ctrl+C to stop the server")
            print()
            
            # Try to open in browser automatically
            try:
                webbrowser.open(f'http://localhost:{port}')
                print("✅ Browser opened automatically")
            except:
                print("ℹ️  Could not open browser automatically")
            
            print()
            httpd.serve_forever()
            
    except OSError as e:
        if e.errno == 98:  # Address already in use
            print(f"❌ Port {port} is already in use. Try a different port:")
            print(f"   python3 serve.py {port + 1}")
        else:
            print(f"❌ Error starting server: {e}")
        sys.exit(1)
    except KeyboardInterrupt:
        print("\n👋 Server stopped")
        sys.exit(0)

if __name__ == "__main__":
    # Get port from command line argument or use default
    port = 8080
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except ValueError:
            print("❌ Invalid port number. Using default port 8080.")
            port = 8080
    
    serve_interface(port)