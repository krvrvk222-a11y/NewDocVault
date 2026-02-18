import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentService } from '../../services/document';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload.html'
})
export class UploadComponent implements OnInit {

  selectedFile: File | null = null;
  documents: any[] = [];
  message = '';

  constructor(private documentService: DocumentService) {}

  ngOnInit(): void {
    this.loadDocuments();
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  uploadFile() {
    if (!this.selectedFile) return;

    this.documentService.upload(this.selectedFile).subscribe({
      next: () => {
        this.message = 'Upload successful!';
        this.loadDocuments();
      },
      error: () => {
        this.message = 'Upload failed';
      }
    });
  }

  loadDocuments() {
    this.documentService.getAll().subscribe((data: any[]) => {
      this.documents = data;
    });
  }

  formatSize(bytes: number): string {
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleString();
  }
}
