import { NextResponse } from "next/server";
import { Resend } from "resend";

const resend = new Resend(process.env.RESEND_API_KEY);

export async function POST(req: Request) {
  try {
    const body = await req.json();
    const { name, email } = body;

    if (!name || !email) {
      return NextResponse.json({ error: "Missing name or email" }, { status: 400 });
    }

    // Send email notification
    const data = await resend.emails.send({
      from: "Qalibrated Systems <noreply@yourdomain.com>", // ⚠️ Replace with your verified domain
      to: "your-email@qalibratedsystems.co.ke", // ⚠️ Your destination email
      subject: "📩 New Catalogue Download Lead",
      html: `
        <h2>New Catalogue Download</h2>
        <p><strong>Name:</strong> ${name}</p>
        <p><strong>Email:</strong> ${email}</p>
        <p><em>Submitted from Qalibrated Systems website.</em></p>
      `,
    });

    console.log("✅ Email sent:", data);

    return NextResponse.json({ success: true });
  } catch (error) {
    console.error("❌ Lead submission error:", error);
    return NextResponse.json({ error: "Failed to send email" }, { status: 500 });
  }
}
