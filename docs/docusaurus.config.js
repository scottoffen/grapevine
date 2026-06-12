// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import { themes as prismThemes } from 'prism-react-renderer';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Grapevine',
  tagline: 'Fast, unopinionated, embeddable, minimalist web framework for .NET',
  favicon: 'img/favicon.ico',

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  // Set the production url of your site here
  url: 'https://scottoffen.github.io',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/grapevine/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'scottoffen', // Usually your GitHub org/user name.
  projectName: 'grapevine', // Usually your repo name.

  onBrokenLinks: 'warn',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          routeBasePath: '/', // Serve docs at the root (no /docs prefix)
          sidebarPath: './sidebars.js',
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://github.com/scottoffen/grapevine/tree/main/docs/',

          lastVersion: 'current', // "current" = whatever is in /docs
          versions: {
            current: {
              label: '6.0',
              path: '/',
            },
            '5.x': {
              label: '5.x',
              path: '5.x',
            },
          }
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // Replace with your project's social card
      image: 'img/docusaurus-social-card.jpg',
      colorMode: {
        respectPrefersColorScheme: true,
      },
      navbar: {
        title: 'Grapevine',
        logo: {
          alt: 'Grapevine Logo',
          src: 'img/logo.png',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'docsSidebar',
            position: 'left',
            label: 'Docs',
          },
          {
            type: 'docsVersionDropdown',
            position: 'right',
            dropdownActiveClassDisabled: true,
          },
          {
            href: 'https://github.com/scottoffen/grapevine',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Documentation',
            items: [
              {
                label: 'Getting Started',
                to: '/',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'Discussions',
                href: 'https://github.com/scottoffen/grapevine/discussions',
              },
              {
                label: 'Stack Overflow',
                href: 'https://stackoverflow.com/questions/tagged/grapevine',
              },
            ],
          },
          {
            title: 'Project',
            items: [
              {
                label: 'Contributing Guide',
                href: 'https://github.com/scottoffen/grapevine/blob/main/CONTRIBUTING.md',
              },
              {
                label: 'Code of Conduct',
                href: 'https://github.com/scottoffen/grapevine/blob/main/CODE_OF_CONDUCT.md',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/scottoffen/grapevine',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Scott Offen`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
      },
      titleDelimiter: '|',
      titleTemplate: 'Grapevine | %s'
    }),

  clientModules: [
    require.resolve('./src/clientModules/version-attribute.js'),
  ],
};

export default config;
