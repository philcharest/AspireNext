import Image from 'next/image';
import styles from './home.module.css';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

interface HomeViewProps {
  weatherData: WeatherForecast[];
  error: string | null;
  serverUrl: string | null;
}

export function HomeView({ weatherData, error, serverUrl }: HomeViewProps) {
  return (
    <main className={styles.main}>
      <Image
        className={styles.logo}
        src="/next.svg"
        alt="Next.js logo"
        width={100}
        height={20}
        priority
      />
      <div className={styles.content}>
        <h1 className={styles.title}>To get started, edit the page.tsx file.</h1>
        <p className={styles.description}>
          Looking for a starting point or more instructions? Head over to{" "}
          <a
            href="https://vercel.com/templates?framework=next.js&utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
            className={styles.link}
          >
            Templates
          </a>{" "}
          or the{" "}
          <a
            href="https://nextjs.org/learn?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
            className={styles.link}
          >
            Learning
          </a>{" "}
          center.
        </p>
      </div>
      <div className={styles.buttons}>
        <a
          className={styles.primaryButton}
          href="https://vercel.com/new?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
          target="_blank"
          rel="noopener noreferrer"
        >
          <Image
            className={styles.buttonIcon}
            src="/vercel.svg"
            alt="Vercel logomark"
            width={16}
            height={16}
          />
          Deploy Now
        </a>
        <a
          className={styles.secondaryButton}
          href="https://nextjs.org/docs?utm_source=create-next-app&utm_medium=appdir-template-tw&utm_campaign=create-next-app"
          target="_blank"
          rel="noopener noreferrer"
        >
          Documentation
        </a>
      </div>

      <WeatherSection serverUrl={serverUrl} weatherData={weatherData} error={error} />
    </main>
  );
}

function WeatherSection({
  serverUrl,
  weatherData,
  error,
}: {
  serverUrl: string | null;
  weatherData: WeatherForecast[];
  error: string | null;
}) {
  return (
    <section className={styles.weatherSection}>
      <h2 className={styles.sectionTitle}>Next.js + .NET Aspire Connection Test</h2>
      <div className={styles.weatherBox}>
        <p className={styles.serverUrl}>
          <strong>Target URL:</strong> {serverUrl || 'Not Found'}
        </p>

        {error ? (
          <p className={styles.error}>Error: {error}</p>
        ) : (
          <ul className={styles.weatherList}>
            {weatherData.map((forecast: any, index: number) => (
              <li key={index} className={styles.weatherItem}>
                {forecast.date}: {forecast.temperatureC}°C - {forecast.summary}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}
