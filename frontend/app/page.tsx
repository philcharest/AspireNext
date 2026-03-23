import { Navigation } from './components/Navigation';
import { HomePage } from './home/HomePage';
import styles from './app.module.css';

export default function Page() {
  return (
    <div className={styles.container}>
      <Navigation />
      <HomePage />
    </div>
  );
}

