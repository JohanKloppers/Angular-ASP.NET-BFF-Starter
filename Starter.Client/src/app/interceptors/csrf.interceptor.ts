import { HttpInterceptorFn } from '@angular/common/http';

const MUTATING = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

export const csrfInterceptor: HttpInterceptorFn = (req, next) => {
    if (!MUTATING.has(req.method)) return next(req);

    const token = getCookie('X-CSRF-TOKEN');
    if (!token) return next(req);

    return next(req.clone({ headers: req.headers.set('X-CSRF-TOKEN', token) }));
};

function getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
    return match ? decodeURIComponent(match[1]) : null;
}
