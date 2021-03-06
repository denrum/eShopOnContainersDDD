import { MuiThemeProvider } from '@material-ui/core/styles';
import Debug from 'debug';
import { Provider } from 'mobx-react';
import * as React from 'react';
import { render } from 'react-dom';
import { hot } from 'react-hot-loader';
import AlertStack from './components/alertStack';
import ApplicationView from './components/applicationView';
import asyncView from './components/asyncView';
import { config } from './config';
import { Modules } from './modules';
import { createRouter } from './router';
import { StoreType } from './stores';

const debug = new Debug('client');

export class Client {
  constructor(private _store: StoreType, private _modules: Modules) {
    _store.history.listen((loc) => this.onLocationChange(loc as any));
    this.onLocationChange(_store.history.location as any);
  }

  public onRenderComplete(route: any, location: UniversalRouterContext) {
    document.title = `${route.title} - ${config.title}`;
  }

  public async onLocationChange(location: UniversalRouterContext) {
    debug('onLocationChange', location);

    let component: any;
    let route: any;
    try {
      route = await createRouter(this._store, this._modules).resolve({
        pathname: location.pathname
      });
      if (route.redirect) {
        this._store.history.push(route.redirect);
        return;
      }
      component = route.component;
    } catch (error) {
      debug('routing exception', error);
      if (error.status === 404) {
        component = React.createElement(asyncView, {
          getComponent: () => import('./components/notFound')
        });
        route = { title: 'Page not found' };
      }
    }

    if (component) {
      const Layout = hot(module)(() => (
        <MuiThemeProvider theme={this._store.theme}>
          <Provider store={this._store}>
            <ApplicationView title={config.title} version={config.build.version}>
              {component}
              <AlertStack/>
            </ApplicationView>
          </Provider>
        </MuiThemeProvider>
      ));
      render(
        <Layout />,
        document.getElementById('application'),
        () => this.onRenderComplete(route, location)
      );

    }
  }
}
