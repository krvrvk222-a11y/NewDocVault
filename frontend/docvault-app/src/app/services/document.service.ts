import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentItem {
  id: string;
  fileName: string;
  sizeBytes: number;
  uploadedAt: string;
  downloadUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {

  private apiUrl = 'http://localhost:5025/api/documents';

  constructor(private http: HttpClient) { }

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(this.apiUrl, formData);
  }

  getDocuments(): Observable<DocumentItem[]> {
    return this.http.get<DocumentItem[]>(this.apiUrl);
  }
}
