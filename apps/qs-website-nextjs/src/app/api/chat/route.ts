import { NextRequest, NextResponse } from "next/server";

interface ChatMessage {
  role: "user" | "assistant" | "system";
  content: string;
}

interface ChatRequestBody {
  messages: ChatMessage[];
}

export async function POST(req: NextRequest) {
  try {
    const body: ChatRequestBody = await req.json();

    const response = await fetch("https://api.openai.com/v1/chat/completions", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${process.env.OPENAI_API_KEY}`,
      },
      body: JSON.stringify({
        model: "gpt-4o-mini",
        messages: body.messages,
        temperature: 0.7,
      }),
    });

    const data = await response.json();
    return NextResponse.json(data);
  } catch (error: unknown) {
    console.error("Chat API error:", error);

    // Optional: safely extract message
    const message = error instanceof Error ? error.message : "Failed to connect to OpenAI";

    return NextResponse.json({ error: message }, { status: 500 });
  }
}
