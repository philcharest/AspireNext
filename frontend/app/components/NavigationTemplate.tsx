import Link from 'next/link';
import styles from './navigation.module.css';

export function NavigationTemplate() {
  return (
    <nav className={styles.nav}>
      <div className={styles.container}>
        <h1 className={styles.brand}>AspireNext</h1>
        <ul className={styles.menu}>
          <li>
            <Link href="/" className={styles.link}>
              Home
            </Link>
          </li>
          <li>
            <Link href="/products" className={styles.link}>
              Products
            </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
}
