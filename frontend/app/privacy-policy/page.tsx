import Link from "next/link";

export const metadata = {
    title: "Privacy Policy — Wall Art Canvases",
};

export default function PrivacyPolicyPage() {
    return (
        <main className="mx-auto max-w-3xl px-6 py-24">
            <p className="gallery-eyebrow">Legal</p>
            <h1 className="mt-3 font-heading text-4xl font-medium tracking-tight text-foreground">
                Privacy Policy
            </h1>

            <div className="mt-8 rounded-md border border-destructive/40 bg-destructive/5 p-4 text-sm text-foreground">
                <strong>Draft template — not legal advice.</strong> Everything in brackets needs your
                input, and this page should be reviewed by a lawyer familiar with Quebec&apos;s Law 25
                before you publish it or process real customer data. It does not cover every
                organizational requirement of Law 25 (e.g. appointing and publishing a designated
                privacy officer, a data-breach response process, or a Privacy Impact Assessment for
                transferring personal information outside Quebec).
            </div>

            <div className="mt-10 space-y-8 text-sm leading-relaxed text-muted-foreground">
                <section>
                    <h2 className="font-heading text-lg text-foreground">1. Who we are</h2>
                    <p className="mt-2">
                        [Business legal name], [address], is responsible for the personal information
                        collected through this website. Our designated person in charge of the protection
                        of personal information, as required under Quebec&apos;s Law 25, can be reached at{" "}
                        [privacy contact email].
                    </p>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">2. What we collect</h2>
                    <p className="mt-2">We collect the following personal information:</p>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                        <li>Account information: your email address and password (stored securely, never in plain text).</li>
                        <li>Order information: items purchased, order history, and shipping/billing details you provide.</li>
                        <li>
                            Payment information: processed directly by our payment provider, Stripe. We
                            never see or store your full card number.
                        </li>
                        <li>Communications: emails you send us, and transactional emails we send you (e.g. order confirmations, password resets).</li>
                        <li>
                            Technical information: strictly necessary cookies for cart and sign-in
                            functionality, and — only if you consent — analytics cookies.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">3. Why we collect it</h2>
                    <p className="mt-2">We use your personal information to:</p>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                        <li>Create and manage your account.</li>
                        <li>Process and fulfill your orders, including payment and returns/refunds.</li>
                        <li>Send you transactional emails about your account and orders.</li>
                        <li>Respond to your questions and support requests.</li>
                        <li>Comply with legal and tax obligations.</li>
                        <li>
                            With your consent only: understand how visitors use the site (analytics) and,
                            in the future, measure marketing campaigns.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">4. Cookies</h2>
                    <p className="mt-2">
                        We use strictly necessary cookies (to keep you signed in and remember your cart)
                        that don&apos;t require consent. If you accept, we also use analytics cookies (e.g.
                        Google Analytics) to understand site usage. You can change your preference at any
                        time using the &quot;Cookie Preferences&quot; link in the footer.
                    </p>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">5. Who we share it with</h2>
                    <p className="mt-2">
                        We share personal information only with service providers who help us run this
                        site, bound by contract to protect it:
                    </p>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                        <li>Stripe (payment processing) — based in the United States.</li>
                        <li>[Your email/SMTP provider] (transactional email) — [location].</li>
                        <li>[Your hosting provider] (application hosting) — [location].</li>
                        <li>
                            Google Analytics (site analytics), only if you&apos;ve consented to analytics
                            cookies — based in the United States.
                        </li>
                    </ul>
                    <p className="mt-2">
                        Some of these providers are located outside Quebec/Canada. [Describe the
                        safeguards in place for these transfers, per your Privacy Impact Assessment.]
                    </p>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">6. How long we keep it</h2>
                    <p className="mt-2">
                        [Describe your actual retention periods — e.g. account data for as long as your
                        account is active, order records for the period required by tax law, etc.]
                    </p>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">7. Your rights</h2>
                    <p className="mt-2">Under Quebec&apos;s Law 25, you have the right to:</p>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                        <li>Access the personal information we hold about you.</li>
                        <li>Request correction of inaccurate information.</li>
                        <li>Withdraw your consent to non-essential cookies at any time.</li>
                        <li>Request that we delete your personal information, subject to legal retention requirements.</li>
                        <li>
                            File a complaint with the{" "}
                            <a
                                href="https://www.cai.gouv.qc.ca/"
                                target="_blank"
                                rel="noreferrer"
                                className="underline underline-offset-4"
                            >
                                Commission d&apos;accès à l&apos;information du Québec
                            </a>
                            .
                        </li>
                    </ul>
                    <p className="mt-2">
                        To exercise any of these rights, contact us at [privacy contact email].
                    </p>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">8. Changes to this policy</h2>
                    <p className="mt-2">
                        We may update this policy from time to time. [State how you&apos;ll notify users of
                        material changes.]
                    </p>
                </section>

                <section>
                    <h2 className="font-heading text-lg text-foreground">9. Contact us</h2>
                    <p className="mt-2">
                        Questions about this policy or your personal information? Contact [privacy contact
                        email].
                    </p>
                </section>
            </div>

            <p className="mt-10 text-sm text-muted-foreground">
                <Link href="/" className="text-primary underline underline-offset-4">
                    Back to home
                </Link>
            </p>
        </main>
    );
}
