import { ModalTheme } from '@/enums';
import { defineStore } from 'pinia';

export const useOverlayStore = defineStore('orverlay', {
  state: () => ({
    dlgLoading: false,
    dlgMessage: false,
    dlgMessageColor: '',
    dlgMessageIcon: '',
    dlgMessageTitle: '',
    dlgMessageText: '',
    dlgServerError: false,
    dlgSessionExpired: false,
    ndwDefault: false,
    snbSessionExpired: false,
    snbTimeLeft: false
  }),
  actions: {
    hideLoadingDialog() {
      this.dlgLoading = false;
    },
    hideMessageDialog() {
      this.dlgMessage = false;
    },
    hideServerErrorDialog() {
      this.dlgServerError = false;
    },
    hideSessionExpiredDialog() {
      this.dlgSessionExpired = false;
    },
    showMessageDialog(theme: ModalTheme, title: string, text: string, delay: number = 0) {
      if (ModalTheme.Ignore) return;

      const themeConfig = {
        [ModalTheme.Ignore]: { icon: 'mdi-dots-horizontal-circle', color: 'grey' },
        [ModalTheme.Warning]: { icon: 'mdi-alert', color: 'warning' },
        [ModalTheme.Error]: { icon: 'mdi-close-octagon', color: 'error' },
        [ModalTheme.Info]: { icon: 'mdi-information-box', color: 'info' },
        [ModalTheme.Success]: { icon: 'mdi-check-decagram', color: 'success' }
      };

      const { icon, color } = themeConfig[theme];

      setTimeout(() => (this.dlgMessage = true), delay);

      this.dlgMessageColor = color;
      this.dlgMessageIcon = icon;
      this.dlgMessageText = text;
      this.dlgMessageTitle = title;
    },
    showErrorMessageDialog(titulo: string, texto: string, delay: number = 0) {
      this.showMessageDialog(ModalTheme.Error, titulo, texto, delay);
    },
    showInfoMessageDialog(titulo: string, texto: string, delay: number = 0) {
      this.showMessageDialog(ModalTheme.Info, titulo, texto, delay);
    },
    showLoadingDialog() {
      this.dlgLoading = true;
    },
    showServerErrorDialog(delay: number = 0) {
      setTimeout(() => (this.dlgServerError = true), delay);
    },
    showSuccessMessageDialog(titulo: string, texto: string, delay: number = 0) {
      this.showMessageDialog(ModalTheme.Success, titulo, texto, delay);
    },
    showWarningMessageDialog(titulo: string, texto: string, delay: number = 0) {
      this.showMessageDialog(ModalTheme.Warning, titulo, texto, delay);
    },
    showSessionExpiredDialog() {
      this.dlgSessionExpired = true;
    }
  }
});
