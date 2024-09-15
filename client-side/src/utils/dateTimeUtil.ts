export const extractDateHours = (dataHora: string, utc: boolean = false) =>
  format(
    dataHora,
    {
      minute: '2-digit',
      hour: '2-digit',
      year: 'numeric',
      day: 'numeric',
      month: 'short'
    },
    utc
  );

export function extractTimeLeft(dateTime0: number, dateTime1: number) {
  const minutes = 1000 * 60;
  const hours = 1000 * 60 * 60;
  const days = hours * 24;

  const difference = dateTime0 - dateTime1 + days;

  return [Math.floor((difference % days) / minutes), Math.floor((difference % minutes) / 1000)];
}

export function extractTimeLeftMessage(dateTime0: string, dateTime1: string | Date) {
  if (typeof dateTime1 === 'string') {
    const date0Local = new Date(dateTime0);
    const date1Local = new Date(dateTime1);

    const date0UtcMs = date0Local.getTime() - date0Local.getTimezoneOffset() * 60 * 1000;
    const date1UtcMs = date1Local.getTime() - date1Local.getTimezoneOffset() * 60 * 1000;

    const timeDifferenceMs = date1UtcMs - date0UtcMs;

    const totalMinutes = Math.floor(timeDifferenceMs / (1000 * 60));
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
  }

  const date0Local = new Date(dateTime0);

  const date0UtcMs = date0Local.getTime() - date0Local.getTimezoneOffset() * 60 * 1000;

  const timeDifferenceMs = dateTime1.getTime() - date0UtcMs;

  const totalMinutes = Math.floor(timeDifferenceMs / (1000 * 60));
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
}

export function formatMillisecondsToTime(milliseconds: number) {
  if (isNaN(milliseconds)) return '...';

  const totalMinutes = Math.floor(milliseconds / (1000 * 60));
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
}

export function getStartOfToday() {
  const now = new Date();
  return new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
}

export function getEndOfToday() {
  const now = new Date();
  return new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1).getTime() - 1;
}

export function getStartOfTodayUtc() {
  const now = new Date();
  return new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate())).getTime();
}

export function getEndOfTodayUtc() {
  const now = new Date();
  return (
    new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() + 1)).getTime() - 1
  );
}

export function isToday(dateTime: string) {
  const date = new Date(dateTime);
  const today = new Date();
  return (
    date.getUTCFullYear() === today.getUTCFullYear() &&
    date.getUTCMonth() === today.getUTCMonth() &&
    date.getUTCDate() === today.getUTCDate()
  );
}

export function isThisMonth(dateTime: string) {
  const date = new Date(dateTime);
  const today = new Date();
  return (
    date.getUTCFullYear() === today.getUTCFullYear() && date.getUTCMonth() === today.getUTCMonth()
  );
}

function format(dataHora: string, formato: object, utc: boolean = false) {
  const date = new Date(dataHora);

  if (utc) return new Date(date.getTime() - date.getTimezoneOffset() * 60 * 1000);
  return date.toLocaleString('en', formato);
}
