import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})

export class TranslationService {
  private testurl = 'https://jsonplaceholder.typicode.com/posts'; // Example API endpoint
  private apiUrl = 'https://localhost:7257/api/translation/translate'; // Corrected API URL

  constructor(private http: HttpClient) {}

  // Example method to fetch posts
  fetchPosts(): Observable<any> {
    return this.http.get(this.testurl).pipe(
      catchError((error) => {
        console.error('Error fetching posts:', error);
        return throwError(() => new Error('Failed to fetch posts. Please try again.'));
      })
    );
  }

  // Modified translateText method to follow a similar pattern as fetchPosts
  translateText(text: string): Observable<any> {
    console.log('URL:', this.apiUrl);

    return this.http.post(this.apiUrl, { text }).pipe( // Send text object directly
      catchError((error) => {
        console.error('An error occurred:', error);
        return throwError(() => new Error('Failed to translate text. Please try again.'));
      })
    );
  }
}