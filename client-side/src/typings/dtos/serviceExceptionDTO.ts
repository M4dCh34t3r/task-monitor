import type { ModalTheme } from '@/enums';

export interface ServiceExceptionDTO {
  theme: ModalTheme;
  title: string;
  text: string;
}
