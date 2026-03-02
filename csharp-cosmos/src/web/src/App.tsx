import { FC } from 'react';
import { BrowserRouter } from 'react-router-dom';
import Layout from './layout/layout';
import './App.css';
import { DarkTheme } from './ux/theme';
import { initializeIcons } from '@fluentui/react/lib/Icons';
import { ThemeProvider } from '@fluentui/react';
import Telemetry from './components/telemetry';

initializeIcons(undefined, { disableWarnings: true });

const App: FC = () => {
  return (
    <ThemeProvider applyTo="body" theme={DarkTheme}>
      <BrowserRouter>
        <Telemetry>
          <Layout />
        </Telemetry>
      </BrowserRouter>
    </ThemeProvider>
  );
};

export default App;
