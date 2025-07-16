# C4 Diagram Assets

This directory contains PNG exports of the C4 diagrams for use in presentations and documentation.

## 📊 **System Context Diagrams**

### **Converting Mermaid to PNG**

You can convert the Mermaid diagrams to PNG using several methods:

#### **Method 1: Mermaid CLI (Recommended)**
```bash
# Install mermaid CLI
npm install -g @mermaid-js/mermaid-cli

# Convert Option 1 (C4Context)
mmdc -i ../01-system-context.md -o system-context-c4.png -t dark -b white

# Convert Option 2 (Flowchart) 
mmdc -i ../01-system-context.md -o system-context-flowchart.png -t dark -b white
```

#### **Method 2: Online Mermaid Live Editor**
1. Go to https://mermaid.live/
2. Copy the mermaid code from the markdown file
3. Paste into the editor
4. Click "Actions" → "Download PNG"

#### **Method 3: VS Code Extension**
1. Install "Mermaid Preview" extension
2. Open the markdown file
3. Right-click on diagram → "Export as PNG"

## 📁 **Expected Files**

After conversion, this directory should contain:
- `system-context-c4.png` - C4Context diagram (Option 1)
- `system-context-flowchart.png` - Flowchart diagram (Option 2)
- `container-architecture.png` - Container architecture diagram
- `component-flows.png` - Component flows diagram
- `service-architectures.png` - Service architecture diagram
- `business-processes.png` - Business process flows
- `onboarding-guide.png` - Onboarding guide diagram

## 🔗 **Usage in Documentation**

Once converted, the PNG files can be referenced in markdown or used in presentations.