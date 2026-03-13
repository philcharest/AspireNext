import Image from "next/image";

export default async function Home() {
    // Aspire injects the URL based on the name "server" used in AppHost.cs
    const serverUrl = process.env['services__server__http__0'] ||
        process.env['services__server__https__0'];

    let weatherData = [];
    let error = null;

    try {
        // Calling the default .NET WeatherForecast endpoint
        const response = await fetch(`${serverUrl}/api/weatherforecast`, {
            cache: 'no-store' // Ensures we get fresh data on every reload
        });

        if (!response.ok) throw new Error(`Server responded with ${response.status}`);

        weatherData = await response.json();
    } catch (e: any) {
        error = e.message;
    }

    return (

        <div className="flex min-h-screen items-center justify-center bg-zinc-50 font-sans dark:bg-black">
            <main className="flex min-h-screen w-full max-w-3xl flex-col items-center justify-between py-32 px-16 bg-white dark:bg-black sm:items-start">
                <Image
                    className="dark:invert"
                    src="/next.svg"
                    alt="Next.js logo"
                    width={100}
                    height={20}
                    priority
                />
                <div className="flex flex-col items-center gap-6 text-center sm:items-start sm:text-left">
                    <h1 className="max-w-xs text-3xl font-semibold leading-10 tracking-tight text-black dark:text-zinc-50">
                        To get started, edit the page.tsx file.
                    </h1>
                    <p className="max-w-md text-lg leading-8 text-zinc-600 dark:text-zinc-400">
                        Looking for a starting point or more instructions? Head over to{" "}
                        <a
                            href="https://vercel.com/templates?framework=next.js&utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
                            className="font-medium text-zinc-950 dark:text-zinc-50"
                        >
                            Templates
                        </a>{" "}
                        or the{" "}
                        <a
                            href="https://nextjs.org/learn?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
                            className="font-medium text-zinc-950 dark:text-zinc-50"
                        >
                            Learning
                        </a>{" "}
                        center.
                    </p>
                </div>
                <div className="flex flex-col gap-4 text-base font-medium sm:flex-row">
                    <a
                        className="flex h-12 w-full items-center justify-center gap-2 rounded-full bg-foreground px-5 text-background transition-colors hover:bg-[#383838] dark:hover:bg-[#ccc] md:w-[158px]"
                        href="https://vercel.com/new?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
                        target="_blank"
                        rel="noopener noreferrer"
                    >
                        <Image
                            className="dark:invert"
                            src="/vercel.svg"
                            alt="Vercel logomark"
                            width={16}
                            height={16}
                        />
                        Deploy Now
                    </a>
                    <a
                        className="flex h-12 w-full items-center justify-center rounded-full border border-solid border-black/[.08] px-5 transition-colors hover:border-transparent hover:bg-black/[.04] dark:border-white/[.145] dark:hover:bg-[#1a1a1a] md:w-[158px]"
                        href="https://nextjs.org/docs?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
                        target="_blank"
                        rel="noopener noreferrer"
                    >
                        Documentation
                    </a>
                </div>
                <h1>Next.js + .NET Aspire Connection Test</h1>

                <section style={{ marginTop: '1rem', padding: '1rem', border: '1px solid #ccc' }}>
                    <p><strong>Target URL:</strong> {serverUrl || 'Not Found'}</p>

                    {error ? (
                        <p style={{ color: 'red' }}>Error: {error}</p>
                    ) : (
                        <ul>
                            {weatherData.map((forecast: any, index: number) => (
                                <li key={index}>
                                    {forecast.date}: {forecast.temperatureC}°C - {forecast.summary}
                                </li>
                            ))}
                        </ul>
                    )}
                </section>
            </main>
        </div>
    );
}

