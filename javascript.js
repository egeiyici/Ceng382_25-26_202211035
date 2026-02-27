document.addEventListener("DOMContentLoaded", () => {
    const btn = document.getElementById("styleBtn");
    const card = document.querySelector(".card");
  
    const titleEl = document.getElementById("cardTitle");
    const textEl = document.getElementById("mainText");
  
    // Eski metinleri saklayalım (geri dönebilmek için)
    const originalTitle = titleEl.textContent;
    const originalText = textEl.innerHTML;
  
    btn.addEventListener("click", () => {
      const isActive = card.classList.toggle("active-style");
  
      if (isActive) {
        titleEl.textContent = "Cristiano Ronaldo’nun İstatistikleri";
        textEl.textContent =
          "Bu sayfa Cristiano Ronaldo’nun son 5 sezondaki gol ve asist istatistiklerini içerir.";
        btn.textContent = "Geri Al";
      } else {
        titleEl.textContent = originalTitle;
        textEl.innerHTML = originalText; // strong etiketleri geri gelsin diye innerHTML
        btn.textContent = "Değiştir";
      }
    });
  });

  document.addEventListener("DOMContentLoaded", function () {

    const btn = document.getElementById("sortFinalBtn");
    const table = document.getElementById("studentTable");
    const tbody = table.querySelector("tbody");
  
    btn.addEventListener("click", function () {
  
      const rows = Array.from(tbody.querySelectorAll("tr"));
  
      rows.sort(function (a, b) {
        const finalA = Number(a.children[3].textContent.trim());
        const finalB = Number(b.children[3].textContent.trim());
        return finalB - finalA; // Büyükten küçüğe
      });
  
      rows.forEach(function (row) {
        tbody.appendChild(row);
      });
  
    });
  
  });

  const img = document.getElementById("mainImage");
const btn = document.getElementById("changeBtn");

const firstImage = "images/image_1.jpg";
const secondImage = "images/image_2.jpg";

btn.addEventListener("click", function () {
  if (img.src.includes("image_1.jpg")) {
    img.src = secondImage;
  } else {
    img.src = firstImage;
  }
});