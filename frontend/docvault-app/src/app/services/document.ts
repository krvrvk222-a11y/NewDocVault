import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {

  private apiUrl = 'http://localhost:5025/api/documents';

  constructor(private http: HttpClient) {}

  upload(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(this.apiUrl, formData);
  }

  getAll() {
    return this.http.get<any[]>(this.apiUrl);
  }
}
