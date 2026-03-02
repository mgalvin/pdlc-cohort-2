import { FC } from 'react';
import { Stack, Text } from '@fluentui/react';
import { stackPadding } from '../ux/styles';

const HomePage: FC = () => {
    return (
        <Stack tokens={stackPadding}>
            <Text variant="xxLarge">Add your own application code</Text>
            <Text variant="medium">
                This is a minimal scaffold with React, C# API, and Cosmos DB. Replace this page and add your own features.
                See the README or <a href="https://learn.microsoft.com/azure/developer/azure-developer-cli/">Azure Developer CLI docs</a> to get started.
            </Text>
        </Stack>
    );
};

export default HomePage;
