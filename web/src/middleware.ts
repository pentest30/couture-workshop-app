import { NextRequest, NextResponse } from 'next/server';

export function middleware(request: NextRequest) {
  const apiUrl = process.env.NEXT_PRIVATE_API_URL || 'http://localhost:5000';
  const { pathname, search } = request.nextUrl;

  const target = `${apiUrl}${pathname}${search}`;

  return NextResponse.rewrite(new URL(target));
}

export const config = {
  matcher: ['/api/:path*', '/uploads/:path*'],
};