using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public class KafkaListenerSettings : Dictionary<string, object>
    {
        public string ConsumerGroup
        {
            get => this["group.id"] as string;
            set => this["group.id"] = value;
        }

        public bool IsAutocommitEnabled
        {
            get
            {
                var value = this["enable.auto.commit"];
                if (value != null && value is bool)
                {
                    return (bool)value;
                }

                return false;
            }
            set
            {
                this["enable.auto.commit"] = value;
            }
        }

        public int? AutoCommitIntervall
        {
            get => this["enable.auto.commit"] as int?;
            set => this["enable.auto.commit"] = value;
        }

        public IEnumerable<string> BootstrapServers
        {
            get => this["bootstrap.servers"] as IEnumerable<string>;
            set => this["bootstrap.servers"] = value;
        }
    }
}
