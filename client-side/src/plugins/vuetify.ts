import '@mdi/font/css/materialdesignicons.css';
import { createVuetify } from 'vuetify';
import * as components from 'vuetify/components';
import 'vuetify/styles';

const defaultLight = {
  dark: false,
  colors: {
    background: '#bbb5bd',
    surface: '#f0ebed',
    mix: '#d5d0d5',
    'mix-inverse': '#1d1b1f',

    primary: '#1c8a77',
    secondary: '#e98832',

    error: '#f44336',
    info: '#2196f3',
    success: '#4caf50',
    warning: '#dba02a'
  }
};

const defaultDark = {
  dark: true,
  colors: {
    background: '#110f10',
    surface: '#2a282e',
    mix: '#1d1b1f',
    'mix-inverse': '#d5d0d5',

    primary: '#33d9b2',
    secondary: '#ff9f43',

    error: '#ff8a92',
    info: '#64b5f6',
    success: '#81c784',
    warning: '#ffb300'
  }
};

export default createVuetify({
  components: components,
  defaults: {
    global: {
      density: 'compact',
      hideDetails: true
    },
    VAutocomplete: {
      itemTitle: 'name',
      itemValue: 'id'
    },
    VDivider: {
      thickness: 4
    },
    VBtn: {
      density: 'default'
    },
    VTable: {
      fixedHeader: true,
      hover: true
    },
    VToolbar: {
      style: 'transition: none;',
      density: 'default'
    }
  },
  theme: {
    themes: {
      defaultLight,
      defaultDark
    }
  }
});
