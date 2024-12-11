import { Component, OnInit, Input } from '@angular/core';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { ImpersonationService } from '../../services/impersonation.service';
import { ISearchPersonnelModel } from '../../models/ISearchPersonnelModel';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-impersonation',
  templateUrl: './impersonation.component.html',
  styleUrls: ['./impersonation.component.css'],
  standalone: true,
  imports: [FormsModule,CommonModule] 
})
export class ImpersonationComponent implements OnInit {
  @Input() clearAfterSelection = true;
  public toImpersonate: ISearchPersonnelModel = <ISearchPersonnelModel>{};
  public personnel$: Observable<ISearchPersonnelModel[]>;
  public searchText = '';
  public loaderCount = 0;
  public searchStatus = '';
  public isSearching = false;
  private searchTerms = new Subject<string>();

  constructor(
    private impersonationService: ImpersonationService,
    private router: Router) {
    this.personnel$ = of();
  }
  ngOnInit(): void {
    this.initializeSearchObserver();
  }

  impersonate(email?: string): void {
    this.router.navigate(['/chat'], { queryParams: { username: email } });
  }

  clearImpersonation(): void {
  }

  onPersonnelSelected(personnel: ISearchPersonnelModel): void {
    this.toImpersonate = personnel;
    if (this.clearAfterSelection) this.searchText = '';
  }

  search(term: string): void {
    this.searchTerms.next(term);
  }

  private initializeSearchObserver(): void {
    this.personnel$ = this.searchTerms.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((term) => {
        this.isSearching = true;
        return term.trim()
          ? this.impersonationService.searchPersonnel(term)
          : of([]);
      })
    );
  
    this.personnel$.subscribe(
      (data) => {
        this.isSearching = false;
      },
      (error) => this.handleError(error)
    );
  }
  

  private handleError(error: any): void {
    console.error('Error:', error.message || 'Server error');
    this.isSearching = false;
    this.loaderCount = 0;
  }
}
