import { FC, ReactElement } from 'react';
import Header from './header';
import { Routes, Route } from 'react-router-dom';
import HomePage from '../pages/homePage';
import { Stack } from '@fluentui/react';
import { headerStackStyles, mainStackStyles, rootStackStyles } from '../ux/styles';

const Layout: FC = (): ReactElement => {
    return (
        <Stack styles={rootStackStyles}>
            <Stack.Item styles={headerStackStyles}>
                <Header />
            </Stack.Item>
            <Stack.Item grow={1} styles={mainStackStyles}>
                <Routes>
                    <Route path="/" element={<HomePage />} />
                </Routes>
            </Stack.Item>
        </Stack>
    );
};

export default Layout;
