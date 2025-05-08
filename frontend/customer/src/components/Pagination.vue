<template>
    <div class = "pagination-container">
      <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="prev-button">
        Previous
      </button>
      <button v-for="page in displayPages" :key="page" @click="changePage(page)" :class="{ active: currentPage === page}">
        {{ page }}
      </button>
      <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="next-button">
        Next
      </button>
    </div>
  </template>
  
  <script setup>
    import { ref, onMounted, computed } from 'vue'
    const emit = defineEmits(['pageChanged'])
    const changePage = (page) => {
      if(page >=1 && page <= totalPages.value){
        emit('pageChanged', page)
      }
    }
    const props = defineProps({
      currentPage: Number,
      pageSize: Number,
      totalProducts: Number,
    })
  
    const totalPages = computed(() => Math.ceil(props.totalProducts / props.pageSize));
    const displayPages = computed(() => {
      const pages = []
      const start = Math.max(1, props.currentPage - 1)
      const end = Math.min(totalPages.value, props.currentPage + 2); // Fixed: Use totalPages.value
      for(let i = start; i <= end; i++){
        pages.push(i)
      }
      return pages;
    });
  </script>
  
  <style scoped>
  .pagination-container {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 20px;
    gap: 10px; /* Tăng khoảng cách giữa các nút */
  }
  
  button {
    margin: 0;
    padding: 10px 15px;
    border: 1px solid #ddd; /* Viền nhẹ */
    background-color: #fff; /* Màu nền trắng */
    color: #333; /* Màu chữ */
    font-size: 14px;
    font-weight: 500;
    border-radius: 5px; /* Bo góc */
    cursor: pointer;
    transition: all 0.3s ease; /* Hiệu ứng hover mượt */
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); /* Hiệu ứng đổ bóng nhẹ */
  }
  
  button:hover:not(:disabled) {
    background-color: #f0f0f0; /* Màu nền khi hover */
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15); /* Tăng đổ bóng khi hover */
  }
  
  
  button.active {
    font-weight: bold;
    background-color: var(--primary-color); /* Màu chính */
    color: white; /* Màu chữ trắng */
    border: none;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2); /* Đổ bóng nổi bật */
  }
  
  button:disabled {
    background-color: #fedefb !important; /* Màu nền khi bị vô hiệu hóa */
    color: #999; /* Màu chữ nhạt */
    cursor: not-allowed;
    box-shadow: none; /* Không có đổ bóng */
  }
  
  button.prev-button,
  button.next-button {
    background-color: var(--primary-color); /* Màu chính */
    color: white; /* Màu chữ trắng */
    border: none;
    padding: 10px 20px;
    font-size: 14px;
    font-weight: 600;
    border-radius: 5px; /* Bo góc */
    transition: all 0.3s ease; /* Hiệu ứng hover mượt */
  }
  
  button.prev-button:hover:not(:disabled),
  button.next-button:hover:not(:disabled) {
    background-color: var(--secondary-color); /* Màu phụ khi hover */
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2); /* Tăng đổ bóng khi hover */
  }
  </style>