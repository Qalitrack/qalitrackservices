
function StatsCounter() {
  const stats = [
    { label: 'Expert Workers', value: 25 },
    { label: 'Happy Clients', value: 500 },
    { label: 'Completed Projects', value: 15 },
    { label: 'Running Projects', value: 10 },
  ];

  return (
    <section className="bg-gray-200 py-16">
      <div className="max-w-6xl mx-auto px-6 grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
        {stats.map((stat, idx) => (
          <div key={idx}>
            <p className="text-4xl font-bold text-amber-400">{stat.value}+</p>
            <p className="mt-2 text-gray-700">{stat.label}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

export default StatsCounter;
