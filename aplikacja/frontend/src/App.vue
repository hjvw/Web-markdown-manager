<template>
  <div>
    <h2>Document Manager</h2>

    <div v-if="documents.length > 0" class="document-list">
      <h3>List of Documents:</h3>
      <ul>
        <li v-for="document in documents" :key="document.documentID">
          <button @click="selectDocument(document.documentID)">
            {{ document.title }}
          </button>
        </li>
      </ul>
    </div>

    <div v-if="selectedDocument && chapters.length > 0" class="chapter-list">
      <h3>Chapters:</h3>
      <ul>
        <li v-for="chapter in chapters" :key="chapter.chapterID">
          <div class="chapter-item">
            <button @click="moveChapter('up', chapter)">Up</button>
            <button @click="moveChapter('down', chapter)">Down</button>
            <button @click="editChapter(chapter)">{{ chapter.title }}</button>
          </div>
        </li>
      </ul>
    </div>

    <div v-if="selectedDocument" class="new-chapter-form">
      <h3>Create New Chapter:</h3>
      <button @click="showNewChapterFields">Add New Chapter</button>

      <div v-if="showNewChapter">
        <div>
          <label for="newChapterTitleInput">Title:</label>
          <input
            type="text"
            id="newChapterTitleInput"
            v-model="newChapter.title"
          />
        </div>
        <div>
          <label for="newChapterTextInput">Text:</label>
          <textarea
            id="newChapterTextInput"
            v-model="newChapter.text"
          ></textarea>
        </div>
        <button @click="createNewChapter">Create New Chapter</button>
      </div>
    </div>

    <div v-if="error">
      <p>Error fetching data: {{ error }}</p>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  data() {
    return {
      documents: [],
      selectedDocument: null,
      chapters: [],
      selectedChapter: null,
      showNewChapter: false,
      error: null,
      newChapter: {
        title: '',
        text: '',
      },
    };
  },
  mounted() {
    this.fetchDocuments();
  },
  methods: {
    async fetchDocuments() {
      try {
        const response = await axios.get('https://localhost:5001/get/documents');
        this.documents = response.data;
        this.selectedDocument = null;
        this.selectedChapter = null;
        this.error = null;
      } catch (error) {
        console.error('Error fetching documents:', error.message);
        this.error = 'Failed to fetch documents';
      }
    },
    async fetchChapters(documentID) {
      try {
        const response = await axios.get(`https://localhost:5001/get/documents/chapters/${documentID-1}`);
        this.chapters = response.data;
        this.selectedChapter = null;
        this.error = null;
      } catch (error) {
        console.error('Error fetching chapters:', error.message);
        this.error = 'Failed to fetch chapters';
      }
    },
    async selectDocument(documentID) {
      this.selectedDocument = documentID;
      this.fetchChapters(documentID);
    },
    async moveChapter(direction, chapter) {
      try {
        const response = await axios.put(`https://localhost:5001/${direction}/chapter/${chapter.chapterID}/position`);
        this.chapters = response.data;
        this.selectedChapter = null;
        this.error = null;
      } catch (error) {
        console.error(`Error moving chapter ${direction}:`, error.message);
        this.error = `Failed to move chapter ${direction}`;
      }
    },
    editChapter(chapter) {
      this.selectedChapter = chapter;
    },
    async updateChapter() {
      try {
        const data = {
          title: this.selectedChapter.title,
          text: this.selectedChapter.text,
        };
        const response = await axios.put(`https://localhost:5001/change/chapter/${this.selectedChapter.chapterID}`, data);
        console.log('Updated chapter:', response.data);
      } catch (error) {
        console.error('Error updating chapter:', error.message);
      }
    },
    showNewChapterFields() {
      this.showNewChapter = true;
    },
    async createNewChapter() {
      try {
        const data = {
          title: this.newChapter.title,
          text: this.newChapter.text,
        };
        const response = await axios.post(`https://localhost:5001/add/document/${this.selectedDocument}/chapter`, data);
        console.log('Created new chapter:', response.data);

       
        this.fetchChapters(this.selectedDocument);


        this.newChapter.title = '';
        this.newChapter.text = '';

        
        this.showNewChapter = false;
      } catch (error) {
        console.error('Error creating new chapter:', error.message);
      }
    },
  },
  
};
</script>

<style scoped>
.document-list {
  float: left;
  width: 30%;
}

.chapter-list {
  float: left;
  width: 60%;
}

.chapter-item {
  margin-bottom: 10px;
}

.chapter-text {
  float: left;
  width: 40%;
}

.new-chapter-form {
  float: left;
  width: 40%;
}
</style>
