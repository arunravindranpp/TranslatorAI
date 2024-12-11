import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ISearchPersonnelModel } from  '../models/ISearchPersonnelModel';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root',
})
export class ImpersonationService {
  private employeeUrl: string;
  constructor(private http: HttpClient, @Inject(ConfigService) private configService: ConfigService) {
    this.employeeUrl = this.configService.get('apiEndpoints').employeeUrl;
  }


  searchPersonnel(term: string): Observable<ISearchPersonnelModel[]> {
    if (!term.trim()) return of([]); // Return empty if search term is empty
    
    const searchUrl = `${this.employeeUrl}/SearchEmployees?search=${encodeURIComponent(term)}`;
    console.log('Search URL:', searchUrl);
  
    return this.http.get<any[]>(searchUrl).pipe(  // Use `any[]` for more flexibility
      map((response) => {
        console.log('Raw API Response:', response); // Log the raw response for debugging
        return response.map(item => ({
          FirstName: item.firstName,   // Mapping API response keys (camelCase) to your model (PascalCase)
          LastName: item.lastName,
          Email: item.email,
          GUI: item.gui
        }));
      }),
      catchError(this.handleError<ISearchPersonnelModel[]>('searchPersonnel', []))
    );
  }
  
  

  private handleError<T>(operation = 'operation', result?: T): (error: any) => Observable<T> {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed: ${error.message}`);
      return of(result as T);
    };
  }
}
