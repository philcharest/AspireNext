interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

export async function fetchWeatherData(serverUrl: string): Promise<{ data: WeatherForecast[]; error: string | null }> {
  let weatherData: WeatherForecast[] = [];
  let error: string | null = null;

  try {
    if (!serverUrl) {
      throw new Error('Server URL not configured');
    }

    const response = await fetch(`${serverUrl}/api/weatherforecast`, {
      cache: 'no-store'
    });

    if (!response.ok) {
      throw new Error(`Server responded with ${response.status}`);
    }

    weatherData = await response.json();
  } catch (e: any) {
    error = e.message;
  }

  return { data: weatherData, error };
}
