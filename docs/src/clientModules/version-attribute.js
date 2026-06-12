// This is a custom module for adding a data-doc-version attribute to
// the html tag, primarily to be able to adjust styles per version.

import ExecutionEnvironment from '@docusaurus/ExecutionEnvironment';

function setDocVersionAttribute(pathname) {

  const path = pathname.replace('/grapevine', '');

  let version = 'current';

  if (path.startsWith('/5.x')) {
    version = '5.x';
  }

  document.documentElement.setAttribute('data-doc-version', version);
}

// Run once on initial load
if (ExecutionEnvironment.canUseDOM) {
  setDocVersionAttribute(window.location.pathname);
}

// Run again on every client-side route change
export function onRouteDidUpdate({location}) {
  if (!ExecutionEnvironment.canUseDOM) {
    return;
  }

  setDocVersionAttribute(location.pathname);
}
