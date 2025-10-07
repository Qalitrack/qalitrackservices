import { NextResponse } from 'next/server';
import { Resend } from 'resend';

export async function POST(req: Request) {
  const { name, email } = await req.json();

  // Save to DB or logs (optional)
  console.log("Lead received:", name, email);

  try {
    if (process.env.RESEND_API_KEY) {
      const resend = new Resend(process.env.RESEND_API_KEY);
      await resend.emails.send({
        from: 'noreply@qalibrated.co.ke',
        to: 'info@qalibrated.co.ke',
        subject: 'New Catalogue Lead',
        html: `<p><strong>Name:</strong> ${name}</p><p><strong>Email:</strong> ${email}</p>`
      });
    } else {
      console.warn("⚠️ RESEND_API_KEY not set — skipping email send.");
    }

    return NextResponse.json({ success: true });
  } catch (error) {
    console.error("Email send failed:", error);
    return NextResponse.json({ success: false }, { status: 500 });
  }
}
