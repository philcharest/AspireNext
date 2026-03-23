import { HomeView } from './HomeView';
import { fetchWeatherData } from './useWeather';

export async function HomePage() {
  const serverUrl = process.env['services__server__http__0'] || process.env['services__server__https__0'] || null;

  const { data: weatherData, error } = await fetchWeatherData(serverUrl || '');

  return <HomeView weatherData={weatherData} error={error} serverUrl={serverUrl} />;
}
